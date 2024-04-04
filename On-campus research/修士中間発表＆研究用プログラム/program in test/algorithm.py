import cv2
import sys
import numpy as np
import matplotlib.pyplot as plt
from sklearn.cluster import DBSCAN
from mpl_toolkits.mplot3d import Axes3D

import segmentation_area_process
import cluster_methods
import normal_vector_process

if __name__ == "__main__":
    # 图片数据的路径
    y_channel_img_path = "./polygon_y.png"         # 原照片黑白图像
    segmentation_img_path = "./segmentation.png"   # segmentation后的图像
    
    # 读取图片数据并转化为RGB色彩空间
    y_channel_img = cv2.imread(y_channel_img_path)
    y_channel_img = cv2.cvtColor(y_channel_img, cv2.COLOR_BGR2GRAY)
    #
    segmentation_img = cv2.imread(segmentation_img_path)
    segmentation_img = cv2.cvtColor(segmentation_img, cv2.COLOR_BGR2RGB)

    # 得到mask图片的颜色和对应的物体领域信息，color[i]的对应领域储存在object_field[i]
    color = segmentation_area_process.Num_Of_Color(segmentation_img)
    object_field = segmentation_area_process.Set_Object_Field(segmentation_img, color)

    # 计算每个像素上的法线向量,格式为 vector[color][y, x, [_y, _x, _v]]  _v是灰度值的差值
    # 向量方向为由中心像素指向最大的相邻像素
    normal_vector = normal_vector_process.Cal_Normal_Vector(object_field, y_channel_img)
    
    # 进行cluster分析,结果返回一个[color][cluster][1]的数组.
    # 也就是说, normal_vector_all 是对所有像素进行clustering并合并同簇内法向量后的结果.每个cluster内只包含一个合并后的结果
    normal_vector_all = cluster_methods.Clustring_DBSCAN(normal_vector)

    print("number of illuminant = {0}, their directional vector is:".format(len(normal_vector_all)))
    for i in normal_vector_all:
        print(i)
    # 合成各个颜色领域中的法线向量
    #syn_normal_vector = Synthetic_Normal(normal_vector)
    #for i in syn_normal_vector:
    #    print(i)
