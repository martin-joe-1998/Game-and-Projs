import sys
import numpy as np

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
    # 中心像素坐标
    center = (1, 1)
    # 所有相邻像素坐标
    neighbors = [sq[i][j] for i in range(3) for j in range(3) if (i, j) != center]

    # 找出相邻像素中的最大值及其索引, max_value_index可以返回复数个最大值的索引位置
    max_value = max(neighbors)
    max_value_index = [(i, j) for i in range(3) for j in range(3) if sq[i][j] == max_value and (i, j) != center]

    # 计算法线向量的合成，注意这里是图像（方格）坐标系,格式为[y, x],此处的y和x是以方块的左上角(0, 0)为标准
    # 当相邻像素中的最大值大于中心像素像素值(gray value),
    if max_value >= sq[1][1]:
        num_max, center_point = len(max_value_index), np.array([1, 1, sq[1][1]])
        if num_max >= 1: # 当最大值有一个以上时，对所有最大值的法线向量进行合成并返回结果
            sq_normal_vector = []
            for i in range(num_max):
                x, y = max_value_index[i][1], max_value_index[i][0]
                # target 是最大值的 y, x, value
                target = np.array([y, x, sq[y][x]])
                # 计算出的法向量被保存在sq_normal_vector
                sq_normal_vector.append((target - center_point).tolist())
            # 返回合成后的法向量
            return list(np.sum(sq_normal_vector, axis=0))
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