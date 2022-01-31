import os
import numpy as np
from scipy.optimize import leastsq
from shapely.geometry import Polygon, mapping
from shapely.ops import triangulate

norm = np.linalg.norm
join = os.path.join

def load_mat(t, f=None, N=-1, M=-1):
    if t is not None:
        t = [float(ti) for ti in t.split(',') if len(ti) > 0]
    else:
        t = []

    tn = np.array(t).reshape((N, M))
    if f is not None:
        np.savetxt(f, tn)
    return tn


def ProjectToImage(cameraToWorldMatrix, intrinsicMatrix, pos, h, w):

    transl = cameraToWorldMatrix[:, -1].reshape((-1, 1))
    Rot = cameraToWorldMatrix[:, :-1]
    WorldTocameraMatrix = np.hstack([Rot.T, Rot.T.dot(-transl)])

    pos1 = np.vstack([pos, np.ones((1, pos.shape[1]))])

    projectionMatrix = np.dot(intrinsicMatrix, WorldTocameraMatrix)
    posList = np.dot(projectionMatrix, pos1)
    posList = posList[:, posList[-1, :] > 0]
    posList = (posList[:-1, :]/posList[-1, :]).T
    posList[:, 1] = h-posList[:, 1]

    ep = 1e-2
    posList = posList[np.logical_and(posList[:, 0] >= -ep, posList[:, 0] <= w+ep)]
    posList = posList[np.logical_and(posList[:, 1] >= -ep, posList[:, 1] <= h+ep)]

    return posList


def multiple_areas(xl,yl,zl,camCenter=None):
    """Calculate total planar area bounded within multiple 3d curves

    Args:
        xl (list): [list of arrays of x coordinates of 3d curves]
        yl (list): [list of arrays of y coordinates of 3d curves]
        zl (list): [list of arrays of z coordinates of 3d curves]

    Returns:
        A_total [float]: [Total area bounded within curvea]
        p_center [numpy array]: [Areas centroid]
    """
    Al = []
    pl = []
    nl = []

    for xi, yi, zi in zip(xl, yl, zl):
        A0, p0, n0 = findArea3D(xi, yi, zi,camCenter=camCenter)
        Al.append(A0)
        pl.append(p0)
        nl.append(n0)

    A_total = sum(Al)
    p_center = sum([pi*Ai for pi, Ai in zip(pl, Al)])/sum(Al)    
    n_center = sum([ni*Ai for ni, Ai in zip(nl, Al)])/sum(Al)    

    return A_total, p_center, n_center

def findArea3D(x, y, z, camCenter=None):
    """Calculate planar area bounded within a 3d curve

    Args:
        x (1 x N numpy array): [x coordinates of curve]
        y (1 x N numpy array): [y coordinates of curve]
        z (1 x N numpy array): [z coordinates of curve]

    Returns:
        A [float]: [Area bounded within curve]
        cen [numpy array]: [Area centroid]
        nz [numpy array]: [Area normal]
    """
    x = np.array(x)
    y = np.array(y)
    z = np.array(z)

    A, B, C, _ = plane_leastsq(x, y, z)

    nz = np.array([A, B, C])/norm([A, B, C])
    x0, y0, z0 = np.mean([x, y, z], axis=1)

    if camCenter is not None:
        dp = np.array([x0,y0,z0]) - camCenter
        if np.dot(dp,nz)<0:
            nz = -1*nz

    x = x - x0
    y = y - y0
    z = z - z0

    if abs(nz.dot([1, 0, 0])) < 1:
        nx = np.cross(nz, [1, 0, 0])
    elif abs(nz.dot([0, 1, 0])) < 1:
        nx = np.cross(nz, [0, 1, 0])
    elif abs(nz.dot([0, 0, 1])) < 1:
        nx = np.cross(nz, [0, 0, 1])

    nx = nx/norm(nx)
    ny = np.cross(nx, nz)

    R = np.array([nx, ny, nz])
    u, v, = np.dot(R, np.array([x, y, z]))[:2, :]

    Area,cen = findArea2D(u,v)    

    cen = np.dot(R.T, cen) + np.array([x0, y0, z0])

    return Area, cen, nz

def findArea2D(u,v):
    """Calculate area bounded within a 2d curve

    Args:
        u (1 x N numpy array): [horz image coordinates of curve]
        v (1 x N numpy array): [vert image coordinates of curve]

    Returns:
        A [float]: [Area bounded within curve]
        cen [numpy array]: [Area centroid]
    """

    points2D = np.vstack([u, v]).T
    polygon = Polygon(points2D)
    if not polygon.is_valid:
        polygon = polygon.buffer(0)    

    A = polygon.area
    cen = np.array([polygon.centroid.x, polygon.centroid.y, 0])

    return A, cen

def plane_leastsq(xs, ys, zs):

    if len(xs) == 3:
        xs = np.insert(xs, 0, np.mean(xs))
        ys = np.insert(ys, 0, np.mean(ys))
        zs = np.insert(zs, 0, np.mean(zs))

    p0 = [1, 1, 1, 1]
    sol = leastsq(plane_error, p0, args=(xs, ys, zs))[0]

    return sol

def plane_error(p, xs, ys, zs):
    A = p[0]
    B = p[1]
    C = p[2]
    D = p[3]
    return abs(A*xs + B*ys + C*zs + D) / np.sqrt(A**2 + B**2 + C**2)


def triangulate_within(polygon):
    if not polygon.is_valid:
        polygon = polygon.buffer(0)
    try:
        return [triangle for triangle in triangulate(polygon) if triangle.within(polygon)]
    except:
        return triangulate(polygon)


def tri_mesh2d(x_coords, y_coords):

    points2D = np.vstack([x_coords, y_coords]).T

    tri = triangulate_within(Polygon(points2D))
    tri_indices = np.zeros((len(tri), 3))
    pt_list = [tuple((xi, yi)) for xi, yi in zip(x_coords, y_coords)]
    pt_set = set(pt_list)

    i = 0
    for t in tri:
        coords = np.array(mapping(t)['coordinates']).reshape(-1, 2)
        tri_set = set([tuple(a) for a in coords])
        j = 0
        for p in pt_set.intersection(tri_set):
            tri_indices[i, j] = pt_list.index(p)
            j += 1
        i += 1
    return tri_indices


def unroll_curves(data):
    xl = []
    yl = []
    zl = []
    for curve in data['curvelist']:
        xl.append([xyz['x'] for xyz in curve['curve']])
        yl.append([xyz['y'] for xyz in curve['curve']])
        zl.append([xyz['z'] for xyz in curve['curve']])

    return xl, yl, zl

