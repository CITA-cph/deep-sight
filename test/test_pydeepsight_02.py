'''
Copyright 2022 CITA

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
'''
import os, math

try:
    import pydeepsight
except ImportError:
    print("Importing local build...")
    import importlib.util, sys
    spec = importlib.util.spec_from_file_location("pydeepsight", os.path.abspath("../bin/pydeepsight.cp{}{}-win_amd64.pyd".format(sys.version_info.major, sys.version_info.minor)))
    pydeepsight = importlib.util.module_from_spec(spec)

from pydeepsight import FloatGrid, IntGrid, VectorGrid, load_scalar_tiff, load_infolog

class Point3d:
    def __init__(self, *args):
        if len(args) == 3:
            self.position = [args[0], args[1], args[2]]
        elif len(args) > 0:
            self.position = [args[0][0], args[0][1], args[0][2]]
        else:
            self.position = [0,0,0]
    def __getitem__(self, key):
        return self.position[key]
    def __setitem__(self, key, value):
        self.position[key] = value
    def __add__(self, p):
        return Point3d(self[0] + p[0], self[1] + p[1], self[2] + p[2])
    def __sub__(self, p):   
        return Point3d(self[0] - p[0], self[1] - p[1], self[2] - p[2])
    def __neg__(self):
        return Point3d(-self[0], -self[1], -self[2])
    def __mul__(self, n):
        return Point3d(self[0] * n, self[1] * n, self[2] * n)
    def x(self):
        return self[0]
    def y(self):
        return self[1]
    def z(self):
        return self[2]

class Line:
    def __init__(self, start : Point3d, end: Point3d):
        self.start = start
        self.end = end

class Polyline:
    def __init__(self, pts):
        self.points = [(p[0], p[1], p[2]) for p in pts]

class AABB:
    def __init__(self, min=(0,0,0), max=(0,0,0)):
        self.min = (min[0], min[1], min[2])
        self.max = (max[0], max[1], max[2])

class Board:
    def __init__(self, id, box):
        self.box = box
        self.id = id

def checkLineBox(box, line):
    b1 = box.min
    b2 = box.max
    l1 = line.start
    l2 = line.end
    hit = Point3d()

    def getIntersection(fDst1, fDst2, p0, p1, hit):
        if (fDst1 * fDst2) >= 0.0: return False
        if (fDst1 == fDst2): return False
        hit.position = Point3d(p0 + (p1 - p0) * (-fDst1/(fDst2-fDst1))).position
        return True

    def inBox(hit, b1, b2, axis):
        if ( axis==1 and hit[2] > b1[2] and hit[2] < b2[2] and hit[1] > b1[1] and hit[1] < b2[1]): return True
        if ( axis==2 and hit[2] > b1[2] and hit[2] < b2[2] and hit[0] > b1[0] and hit[0] < b2[0]): return True
        if ( axis==3 and hit[0] > b1[0] and hit[0] < b2[0] and hit[1] > b1[1] and hit[1] < b2[1]): return True
        return False

    if (l2[0] < b1[0] and l1[0] < b1[0]): return False
    if (l2[0] > b2[0] and l1[0] > b2[0]): return False
    if (l2[1] < b1[1] and l1[1] < b1[1]): return False
    if (l2[1] > b2[1] and l1[1] > b2[1]): return False
    if (l2[2] < b1[2] and l1[2] < b1[2]): return False
    if (l2[2] > b2[2] and l1[2] > b2[2]): return False
    if (l1[0] > b1[0] and l1[0] < b2[0] and
        l1[1] > b1[1] and l1[1] < b2[1] and
        l1[2] > b1[2] and l1[2] < b2[2]):
            hit = l1
            return True
    if ((getIntersection( l1[0]-b1[0], l2[0]-b1[0], l1, l2, hit) and inBox( hit, b1, b2, 1 ))
      or (getIntersection( l1[1]-b1[1], l2[1]-b1[1], l1, l2, hit) and inBox( hit, b1, b2, 2 )) 
      or (getIntersection( l1[2]-b1[2], l2[2]-b1[2], l1, l2, hit) and inBox( hit, b1, b2, 3 )) 
      or (getIntersection( l1[0]-b2[0], l2[0]-b2[0], l1, l2, hit) and inBox( hit, b1, b2, 1 )) 
      or (getIntersection( l1[1]-b2[1], l2[1]-b2[1], l1, l2, hit) and inBox( hit, b1, b2, 2 )) 
      or (getIntersection( l1[2]-b2[2], l2[2]-b2[2], l1, l2, hit) and inBox( hit, b1, b2, 3 ))):
        return True

    return False

def write_knots(path, knots):
    with open(path, 'w') as file:
        for k in knots:
            file.write("{},{},{},{},{},{},{},{}\n".format(k.index, k.start[0], k.start[1], k.start[2],
                k.end[0], k.end[1], k.end[2], math.atan(k.radius / k.length)))

def write_pith(path, pith):
    with open(path, 'w') as file:
        for i in range(len(pith) - 1):
            p0 = pith[i]
            p1 = pith[i+1]
            file.write("{},{},{},{},{},{}\n".format(p0[0], p0[1], i, p1[0], p1[1], i+1))

def write_boards(path, boards):
    with open(path, 'w') as file:
        for board in boards:
            file.write("{},{},{},{},{},{},{}".format(board.id, 
                board.box.min[0], board.box.min[1], board.box.min[2],
                board.box.max[0], board.box.max[1], board.box.max[2]))

def main():
    tiff_path = os.path.expandvars(r"%rawlamdata%\Microtec\20220302.092014.DK.feb.log04_char\infolog@20220302.092014.DK.feb.log04_char.tiff")
    ilog = load_infolog(tiff_path, True)

    directory = os.path.dirname(tiff_path)
    basename = os.path.splitext(os.path.basename(tiff_path))[0]
    basename = basename.split("@")[-1]

    boards = [
        Board(0, AABB((-20,-165,0), (125, -110, 4000))),
        Board(1, AABB((30,-165,0), (155, -110, 4000))),
        Board(2, AABB((-80,-35,0), (25, 10, 4000))),
        ]

    boards = []

    knot_path = os.path.join(directory, "knots@{}.txt".format(basename))
    write_knots(knot_path, ilog.knots)

    for b in boards:
        knots = []

        for k in ilog.knots:
            if (checkLineBox(b.box, Line(Point3d(k.start), Point3d(k.end)))):
                knots.append(k)

        knot_path = os.path.join(directory, "knots{}@{}.txt".format(b.id, basename))
        write_knots(knot_path, knots)

        board_path = os.path.join(directory, "board{}@{}.txt".format(b.id, basename))
        write_boards(board_path, [b])

    pith_path = os.path.join(directory, "pith@{}.txt".format(basename))
    write_pith(pith_path, ilog.pith)
if __name__=="__main__":
    main()