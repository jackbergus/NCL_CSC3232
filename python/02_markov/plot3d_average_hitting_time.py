#!/usr/bin/env python3
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import pandas as pd
import os
import sys

fileDir = sys.argv[1]

x = sys.argv[2]
y = sys.argv[3]
z = sys.argv[4]
data = pd.read_csv(fileDir, engine = 'c', float_precision = 'round_trip', dtype=np.float64)

dataTop = data.drop_duplicates(subset=[x,y], keep='first', inplace=False)
XTop = dataTop[x]
YTop = dataTop[y]
ZTop = dataTop[z]

dataMid = data.drop_duplicates(subset=[x,y], keep=False, inplace=False)
XMid = dataMid[x]
YMid = dataMid[y]
ZMid = dataMid[z]

dataBottom = data.drop_duplicates(subset=[x,y], keep='last', inplace=False)
XBottom = dataBottom[x]
YBottom = dataBottom[y]
ZBottom = dataBottom[z]

fig = plt.figure(figsize=(11.5, 8.5))
ax = fig.add_subplot(111, projection='3d')

ax.plot_trisurf(XTop, YTop, ZTop, cmap='viridis', alpha=0.5)
ax.plot_trisurf(XMid, YMid, ZMid, cmap='viridis', alpha=0.5)
ax.plot_trisurf(XBottom, YBottom, ZBottom, cmap='viridis', alpha=0.5)

plt.xlabel("p")
plt.ylabel("q")
plt.title("Average Hitting Time")

plt.show()
