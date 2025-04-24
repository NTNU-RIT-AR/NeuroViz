from neuroviz import NeuroViz, ParameterDict
from itertools import combinations

neuro = NeuroViz(port=9001, use_secret=False)

def preset(transparency: float) -> ParameterDict:
    return {
        "transparency": transparency,
        "see_through": 0.2,
        "outline": 0.5,
        "smoothness": 1
    }

transparencies = [x / 10 for x in range(0, 11)]
presets = [preset(t) for t in transparencies]

for (a, b) in combinations(presets, 2):
    chosen = neuro.prompt_choice(a, b)
    picked_a = chosen == a

    if picked_a:
        print("User picked preset A: ", a)
    else:
        print("User picked preset B: ", b)
