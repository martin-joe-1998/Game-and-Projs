import cv2
import sys
import numpy as np
import matplotlib.pyplot as plt
from sklearn.cluster import DBSCAN
from mpl_toolkits.mplot3d import Axes3D

# 把mask图片中所有的颜色都存储到color并返回
def Num_Of_Color(mask):
    color = []
    height, width, channel = mask.shape

    for h in range(height):
        for w in range(width):
            current_color = mask[h][w].tolist()
            # 检查整个颜色是否存在于列表中
            if current_color not in color:
                color.append(current_color)

    #print("color:", color, "num of color:", len(color))
    return color


# 根据color的信息，找出每个颜色对应的领域里各个像素的坐标，储存到object_field并返回 
def Set_Object_Field(mask, color):
    num = len(color)
    object_field = [[] for _ in range(num)]
    height, width, channel = mask.shape

    for h in range(height):
        for w in range(width):
            current_color = mask[h][w].tolist()
            index = color.index(current_color)
            object_field[index].append([h, w])
    '''
    for i in range(len(object_field)):
        print("size of field",i,":",len(object_field[i]))
    '''
    return object_field 
    

# 遍历各个物体领域，计算每个像素的法线向量,返回的法线向量数组应该遵从 [color][height, width, _v] 的格式
def Cal_Normal_Vector(o_f, img):  # o_f : [color][height, width]   img : [height][width][y]
    normal = [[] for _ in range(len(o_f))] # 法线向量, len(o_f) = num of color

    for color in range(len(o_f)):
        coord_dict = {(x, y): index for index, (x, y) in enumerate(o_f[color])}
        for coord in range(len(o_f[color])):
            sys.stdout.write("\rProcessing: {} / {}".format(coord + 1, len(o_f[color])))
            square = [[0, 0, 0], [0, 0, 0], [0, 0, 0]] # 计算法线向量的九宫格
            y, x = o_f[color][coord][0], o_f[color][coord][1] # x, y 是图像空间下的坐标
            if (y-1, x-1) in coord_dict: square[0][0] = img[y-1][x-1]
            if (y-1, x) in coord_dict: square[0][1] = img[y-1][x]
            if (y-1, x+1) in coord_dict: square[0][2] = img[y-1][x+1]
            if (y, x-1) in coord_dict: square[1][0] = img[y][x-1]
            square[1][1] = img[y][x]
            if (y, x+1) in coord_dict: square[1][2] = img[y][x+1]
            if (y+1, x-1) in coord_dict: square[2][0] = img[y+1][x-1]
            if (y+1, x) in coord_dict: square[2][1] = img[y+1][x]
            if (y+1, x+1) in coord_dict: square[2][2] = img[y+1][x+1]
            res = Cal_Square_Window(square) # res是[_y, _x, _y]的数组
            normal[color].append([y, x, res])
            sys.stdout.flush()  # 刷新缓冲区
        print()
            
    print("\nProcessing completed.")
    return normal


# 接受一个3x3的正方形数组，计算单个中心像素的法向量并返回[y, x, _value_]
def Cal_Square_Window(sq):
    center = (1, 1)
    neighbors = [sq[i][j] for i in range(3) for j in range(3) if (i, j) != center]

    # 找出最大值及其索引, max_index可以返回复数个最大值的索引位置
    max_value = max(neighbors)
    max_index = [(i, j) for i in range(3) for j in range(3) if sq[i][j] == max_value and (i, j) != center]

    # 计算法线向量的合成，注意这里是图像（方格）坐标系，格式为[y, x],此处的y和x是以方块的左上角(0, 0)为标准
    if max_value >= sq[1][1]:
        num_max, center_point = len(max_index), np.array([1, 1, sq[1][1]])
        if num_max >= 1: # 当最大值有一个以上时，对所有最大值的法线向量进行合成并返回结果
            all_vector = []
            for i in range(num_max):
                x, y = max_index[i][1], max_index[i][0]
                target = np.array([y, x, sq[y][x]])
                all_vector.append((target - center_point).tolist())
            return list(np.sum(all_vector, axis=0))
        else:
            print("error happened. num_max = {}".format(num_max))
    else: # 中心像素就是唯一的最大值，返回0向量
        return [0, 0, 0]


# 合成各个颜色区域的法线向量
def Synthetic_Normal(n_v):
    ret = []
    res = np.array([0, 0, 0])
    for col in range(len(n_v)):
        for num in range(len(n_v[col])):
            res += np.array(n_v[col][num][2])
        res = np.round(res / len(n_v[col]), 4) 
        ret.append(res.tolist())
    return ret


# 对各个颜色内的所有像素上的法线向量进行cluster分析，再合并同cluster内的所有向量
# n_v形状: color x num x [y, x, [_y, _x, _v]]  num是该颜色领域内所有点的数量
def Clustring_DBSCAN(n_v):
    # 用来容纳预处理数据的容器
    data = [[] for _ in range(len(n_v))]

    # 首先遍历每个颜色领域，进行clustering并合并同簇内的所有向量
    for col in range(len(n_v)):
        # 扁平化数据，[y, x, [_y, _x, _v]] -> [y, x, _y, _x, _v], len(flat) = num
        flattened_data = np.array([np.concatenate([item[:2], item[2]]) for item in n_v[col]])

        # 使用DBSCAN进行聚类
        # 较大的 eps 和 min_samples 可能会减少噪声点的数量, 但也可能导致将一些真实的簇合并在一起
        dbscan = DBSCAN(eps=5, min_samples=51)
        # labels里按原顺序容纳每个样本坐标所属的cluster, 比如[0, 1, 1, 2, -1], -1代表噪声数据
        labels = dbscan.fit_predict(flattened_data)
        # set()返回有序无重复的set类型
        unique_label_list = list(set(labels))
        # 创建一个容纳单个col领域里每个cluster的容器
        syn_data = [[] for _ in range(len(unique_label_list))]
        print("color", col, ":",len(syn_data), unique_label_list)
        # 根据每个样本坐标被分配的labels,将其放进相应的数组单元里
        for i, v in enumerate(labels):
            index = unique_label_list.index(v)
            syn_data[index].append(flattened_data[i])
        # 合成各个cluster内的所有向量,计算后syn_data的长度应该等于cluster的数量
        for i in range(len(syn_data)):
            length = len(syn_data[i])
            syn_data[i] = list(np.round(np.sum(syn_data[i], axis=0) / length, decimals=3)) # 保留小数点后三位
        data[col] = syn_data
    print()
    # 将各个物体领域里合并后的cluster向量扁平化，此时没有排除噪声
    flat_list = [item for sublist in data for item in sublist]
    # 此处针对全图处理，数据少且坐标比较分散，采取较大的eps和较小的min_sample比较合适
    dbscan_all = DBSCAN(eps=15, min_samples=2)
    labels_all = dbscan_all.fit_predict(flat_list)
    unique_label_all_list = list(set(labels_all))
    print("full img:", len(unique_label_all_list), unique_label_all_list)

    ret = [[] for _ in range(len(unique_label_all_list))]
    for i, v in enumerate(labels_all):
        index = unique_label_all_list.index(v)
        ret[index].append(flat_list[i])
    # 合并图片整体的各个cluster内的向量
    for i in range(len(ret)):
        length = len(ret[i])
        ret[i] = list(np.round(np.sum(ret[i], axis=0) / length, decimals=3)) # 保留小数点后三位
    # 返回除噪音数据（最后一个分类为-1的）以外的cluster合并结果
    return ret[:-1]


if __name__ == "__main__":
    # 图片数据的路径
    y_channel_img_path = "./polygon_y.png"
    segmentation_img_path = "./segmentation.png"
    
    # 读取图片数据并转化为RGB色彩空间
    y_channel_img = cv2.imread(y_channel_img_path)
    y_channel_img = cv2.cvtColor(y_channel_img, cv2.COLOR_BGR2GRAY)
    #
    segmentation_img = cv2.imread(segmentation_img_path)
    segmentation_img = cv2.cvtColor(segmentation_img, cv2.COLOR_BGR2RGB)

    # 得到mask图片的颜色和对应的物体领域信息，color[i]的对应领域储存在object_field[i]
    color = Num_Of_Color(segmentation_img)
    object_field = Set_Object_Field(segmentation_img, color)
    # 计算每个像素上的法线向量,格式为 vector[color][y, x, [_y, _x, _v]]  _v是灰度值的差值
    # 向量方向为由中心像素指向最大的相邻像素
    normal_vector = Cal_Normal_Vector(object_field, y_channel_img)
    
    # 进行cluster分析，结果返回一个[color][cluster][1]的数组
    normal_vector_all = Clustring_DBSCAN(normal_vector)

    print("number of illuminant = {0}, their directional vector is:".format(len(normal_vector_all)))
    for i in normal_vector_all:
        print(i)
    # 合成各个颜色领域中的法线向量
    #syn_normal_vector = Synthetic_Normal(normal_vector)
    #for i in syn_normal_vector:
    #    print(i)
