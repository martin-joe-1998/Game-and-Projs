import numpy as np
from sklearn.cluster import DBSCAN

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