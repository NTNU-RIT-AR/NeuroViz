from neuroviz import NeuroViz, ParameterDict, default_parameters
from itertools import combinations

neuro = NeuroViz(port=9001, use_secret=False)

def preset(smoothness: float) -> ParameterDict:
    return {
        **default_parameters(),
        "smoothness": smoothness,
    }

smoothnesses = [x / 10 for x in range(0, 11)]
presets = [preset(t) for t in smoothnesses]

for (a, b) in combinations(presets, 2):
    chosen = neuro.prompt_choice(a, b)
    picked_a = chosen == a

    if picked_a:
        print("User picked preset A: ", a)
    else:
        print("User picked preset B: ", b)
