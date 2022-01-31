import os
import json
import cv2
import numpy as np
import copy
from geometry import load_mat, ProjectToImage, tri_mesh2d, unroll_curves, multiple_areas
join = os.path.join

def readImage(img_bytes, ServerClass, headers):
    
    out_path = ServerClass.out_path
    i_img = ServerClass.i
    img = cv2.imdecode(np.frombuffer(img_bytes, dtype='uint8'), 1)

    save=join(out_path,"CaptureImage%i.jpg" % i_img) 
    cv2.imwrite(save, img)
            
    h, w, _ = img.shape

    w2 = int(img.shape[1] * ServerClass.scale)
    h2 = int(img.shape[0] * ServerClass.scale)
    dim = (w2, h2)
    img = cv2.resize(img, dim)

    if headers.get('cameraToWorldMatrix') is not None:
        cameraToWorldMatrix = load_mat(headers.get('cameraToWorldMatrix'),
                                       f=join(out_path,"cameraToWorldMatrix%i.txt" % i_img),N=4, M=4)[:-1, :]

        intrinsicMatrix = load_mat(headers.get('intrinsicMatrix'), f=join(out_path,"intrinsicMatrix.txt"), N=4, M=4)[:-1, :-1]

        pos = load_mat(headers.get('posList'), M=3).T
        neg = load_mat(headers.get('negList'), M=3).T

        posList = ProjectToImage(cameraToWorldMatrix, intrinsicMatrix, pos, h, w)
        negList = ProjectToImage(cameraToWorldMatrix, intrinsicMatrix, neg, h, w)
    else:
        posList = load_mat(headers.get('posList'), M=2)
        negList = load_mat(headers.get('negList'), M=2)

    ptList = np.vstack([posList, negList])

    x_coords = ptList[:, 0]*ServerClass.scale
    y_coords = ptList[:, 1]*ServerClass.scale
    is_pos = np.hstack([np.ones(posList.shape[0]), np.zeros(negList.shape[0])])

    if len(is_pos) > 0:
        mask = ServerClass.engine.predict(x_coords, y_coords, is_pos, img, ServerClass.threshold, 
                                          save_path=join(out_path,'mask%i.png' % i_img),
                                          brs_mode=ServerClass.brs_mode)
        contours, hierarchy = cv2.findContours(
            mask, cv2.RETR_TREE, cv2.CHAIN_APPROX_SIMPLE)
    else:
        contours = []

    objects = []
    for c in contours:
        con = c.reshape(-1, 2)
        if con.shape[0] < 3:
            continue

        obj = dict([])
        obj['coords'] = []
        obj['id'] = ''
        obj['triangles'] = list(tri_mesh2d(con[:, 0], con[:, 1]).reshape(-1))
        for pt in con:
            coords = dict([])
            coords['x'] = float(pt[0])/ServerClass.scale
            coords['y'] = float(pt[1])/ServerClass.scale
            obj['coords'].append(coords)
        objects.append(obj)


    out = {'metadata': {'width': w, 'height': h, 'format': ''},
           'requestId': '', 'objects': objects}

    return out

def analyze_area(data, ServerClass, headers, save = None):
    """[summary]

    Args:
        data ([bytes]): [byte data recieved from http request, contains curves points]

    Returns:
        [dict]: [data to be sent back, containing area and centroid]
    """
    out_path = ServerClass.out_path
    
    i = ServerClass.i        
    curves=json.loads(data)

    xl, yl, zl = unroll_curves(curves)

    cameraToWorldMatrix = load_mat(headers.get('cameraToWorldMatrix'), N=4, M=4)
    C = cameraToWorldMatrix[:-1,-1]
    A_total, p_center, n_center = multiple_areas(xl,yl,zl,camCenter=C)

    out = dict([])
    out['area'] = A_total
    out['x'] = p_center[0]
    out['y'] = p_center[1]
    out['z'] = p_center[2]
    out['nx'] = n_center[0]
    out['ny'] = n_center[1]
    out['nz'] = n_center[2]    

    if save is not None:
        with open(save, 'w') as json_file:
            out2 = copy.copy(out)
            out2.update(curves)
            json.dump(out2, json_file)     
    return out    
