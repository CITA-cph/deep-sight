# -*- coding: utf-8 -*-
"""
Created on Wed Apr 15 11:45:53 2020

@author: Phil.Ayres
"""

import pandas as pd
import networkx as nx
#import matplotlib.pyplot as plt
#import mpl_toolkits.mplot3d as plt3d
from itertools import islice

def FindAllSourceNodes(G, weight=None):    
    terminalNodes =[]
    sources = []    
    # Analyse the degree of each node and return a list of degree 1 nodes
    dList = list(G.degree(nodes))
    for i,d in enumerate(dList):
        if d[1] == 1:
            terminalNodes.append(d[0])
#    print(len(terminalNodes))
    for t in terminalNodes:       
        spl = nx.shortest_path_length(G,source=t, weight=weight)
        max_value = max(spl.values())#; 
        k = {key for key, value in spl.items() if value == max_value}
        sources.extend(k)
    return list(set(sources))

def FindAllSinkNodes(G, sources, weight=None):
    sinks = []
    for s in sources:
        spl = nx.shortest_path_length(G,source=s, weight=weight)
        max_value = max(spl.values())#; 
        k = {key for key, value in spl.items() if value == max_value}
        sinks.append(list(k))
    return sinks

def SimplePaths(G, sources, sinks):
    simplePaths = []
    for i, s in enumerate(sources):
        for t in sinks[i]:
            asp = nx.all_simple_paths(G, source=s, target=t)
            simplePaths.extend(list(asp))
    return simplePaths

def k_degree(G, val):
	#kdNodes.append([d[0] for d in dList if d[1] == 5])
	#esmall = [(u, v) for (u, v, d) in G.edges(data=True) if d['weight'] <= 0.5]
    kdNodes =[]
    dList = list(G.degree(G.nodes))
    for i,d in enumerate(dList):
        if d[1] == val:
            kdNodes.append(int(d[0]))
    return kdNodes

def k_shortest_paths(G, source, target, k, weight=None):
    return list(islice(nx.shortest_simple_paths(G, source, target, weight=weight), k))

def SimplePathsAsEdges(G, paths):
    simpleEdges = []
    for path in map(nx.utils.pairwise, paths):
        simpleEdges.append(list(path))
    return simpleEdges


def BuildGraph(f_path):
	# Get the graph data from the excel sheet
	df = pd.ExcelFile(f_path).parse('Branch_Info') 
	# Subset the dataframe as required
	df = df[21:4743]
	
	# Create a graph object instance
	G = nx.Graph()

	# Get the unique set of node indices & edges
	ni = list(zip(df['V1/P1 index'], df['V2/P2 index']))
	nodes = list(set(list(sum(ni, ()))))
	edges = list(zip(df['V1/P1 index'], df['V2/P2 index'], df['RhinoGH Euc Dist (voxels)']))

	# Build the graph
	G.add_nodes_from(nodes)
	#G.add_edges_from(edges)
	G.add_weighted_edges_from(edges)

	return G


def main():
	f_path = r'C:\Users\phil.ayres\Desktop\glaAI_from_Pdrive\AnalysisResults_glaAI_param_2-2-50_rev1.xlsx'
	G = BuildGraph(f_path)
	#print(G.nodes)

	print(k_degree(G,4))


if __name__ == "__main__":

    main()






# **** Analyse the graph
#uniqueSources = FindAllSourceNodes(G, 'weight')
#print(uniqueSources)

#sinksForSources = FindAllSinkNodes(G, uniqueSources, 'weight')
#sinksForSources = [item[0] for item in sinksForSources]
#print(sinksForSources)


#spl = []
#cpIndices = [[2808, 962], [129, 5591]]
#for i in range(len(cpIndices)):
#    spl.extend(k_shortest_paths(G, cpIndices[i][0], cpIndices[i][1], 1, 'weight'))
#print(len(list(spl)))
#print(list(spl))

#kdl = k_degree(G, 1)
#print(len(kdl))
#print(kdl)

##print(nx.shortest_path(G,2208,2218,'weight'))


#sssp = nx.single_source_shortest_path(G,34)
#spl = sssp.values()
#print(spl, len(spl))
#l =[]
#l.append([len(i) for i in spl if len(i) < 6])
#print(l)

"""
simplePathsNodeList = SimplePaths(G, uniqueSources, sinksForSources)        
print(len(simplePathsNodeList))
print(simplePathsNodeList)

simplePathsEdgeList = SimplePathsAsEdges(G, sp)
print(len(simplePathsEdgeList))
print(simplePathsEdgeList)

pathLengths = []

#pathLengths.append(len(p) for p in simplePathsEdgeList)
#print(pathLengths)

for p in simplePathsEdgeList:
    print(len(p))
"""


"""
# VISUALISE THE NETWORK
xtl = list(zip(df['P1 x'], df['P2 x']))
ytl = list(zip(df['P1 y'], df['P2 y']))
ztl = list(zip(df['P1 z'], df['P2 z']))

fig = plt.figure()
fig.set_size_inches(8,8)
ax = fig.add_subplot(111, projection='3d', aspect='equal')
ax.set_xlim([0, 850])
ax.set_ylim([0, 850])
ax.set_zlim([0, 400])
color = 'b'
linewidth = 0.3

# Plot the full network
for i, (xt, yt, zt) in enumerate(zip(xtl, ytl, ztl)):
    line = plt3d.art3d.Line3D(xt,yt,zt, color=color, linewidth=linewidth)
    ax.add_line(line)
    edges.append(line)

# Plot overlays
#for i in range(22):
#    ax.add_line(edges)


#lc = LineCollection(edges, color=color, linewidth=linewidth)
#ax.add_collection(lc)

plt.show()

"""



