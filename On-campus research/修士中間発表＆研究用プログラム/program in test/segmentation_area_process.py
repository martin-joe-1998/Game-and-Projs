# 把mask图片中所有的颜色都存储到color并返回
# 最终返回的color是保存了所有颜色的rgb信息的一维数组,其长度等于颜色的数量
# 颜色的数量又等于被segmentation的区域的数量
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


# 根据 color 的信息，找出每个颜色对应的领域里各个像素的坐标，储存到 object_field 并返回
# 最终返回的 object_field 是形状为[color_index][number of coord of this color]的二维数组，color_index 对应输入里 color 数组中的同样 index 的颜色。
# object_field 应该包含 segmentation图像 中所有的像素坐标
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
        print("size of field", i,":", len(object_field[i]))
    '''
    return object_field 