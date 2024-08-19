#old script, merged with semantic_sim.py

#use this script to rescale similarity matrices for majors, minors, and industries
#specify or alter paths of csv files (similarity matrices generated from semanticc_sim.py) if needed
#keeps all exact matches (diagonals of matrices) as 1, rescales all other values to be between 0 and 0.6
#results are saved to new csv files

import pandas as pd
import numpy as np

majorSimMatrix = pd.read_csv('/content/majorsSimMatrix.csv')
minorSimMatrix = pd.read_csv('/content/minorsSimMatrix.csv')
industriesSimMatrix = pd.read_csv('/content/industriesSimMatrix.csv')

majorSimMatrix = majorSimMatrix.drop(majorSimMatrix.columns[0], axis=1)
minorSimMatrix = minorSimMatrix.drop(minorSimMatrix.columns[0], axis=1)
industriesSimMatrix = industriesSimMatrix.drop(industriesSimMatrix.columns[0], axis=1)

majorSimMatrix.head()

#scaling:
#keep exact pairings as 1
#rescale all other values to be between 0 and 0.6

np.fill_diagonal(majorSimMatrix.values, 1)
np.fill_diagonal(minorSimMatrix.values, 1)
np.fill_diagonal(industriesSimMatrix.values, 1)

def rescale_values(values, new_min, new_max):
    old_min = np.min(values)
    old_max = np.max(values)
    return (new_max - new_min) * (values - old_min) / (old_max - old_min) + new_min

mask = majorSimMatrix != 1
rescaled_majors = majorSimMatrix.copy()
rescaled_majors[mask] = rescale_values(majorSimMatrix[mask], 0, 0.6)

mask = minorSimMatrix != 1
rescaled_minors = minorSimMatrix.copy()
rescaled_minors[mask] = rescale_values(minorSimMatrix[mask], 0, 0.6)

mask = industriesSimMatrix != 1
rescaled_industries = industriesSimMatrix.copy()
rescaled_industries[mask] = rescale_values(industriesSimMatrix[mask], 0, 0.6)

rescaled_majors.head()

rescaled_majors.to_csv('rescaled_majors.csv', index=False)
rescaled_minors.to_csv('rescaled_minors.csv', index=False)
rescaled_industries.to_csv('rescaled_industries.csv', index=False)