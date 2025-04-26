from neuroviz import NeuroViz, ParameterDict
from itertools import combinations

neuro = NeuroViz(port=9001, use_secret=False)

def preset(transparency: float) -> ParameterDict:
    return {
        "transparency": transparency,
        "glow": 0.2,
        "smoothness": 1.0,
        "emission": 0.5,
        "light_intensity": 1.0,
        "light_temperature": 6500.0
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
