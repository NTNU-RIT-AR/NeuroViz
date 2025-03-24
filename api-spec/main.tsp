import "@typespec/http";
import "@typespec/sse";

using TypeSpec.Http;
using TypeSpec.SSE;

@service(#{ title: "NeuroViz Service" })
namespace NeuroVizService;

model Parameters {
  hue: float,
  smoothness: float,
  metallic: float,
  emission: float,
}

model ExperimentPrompt {
  experiment_type: "choice" | "rating";
  preset: Parameters;
}

@discriminated(#{ envelope: "none", discriminatorPropertyName: "kind" })
union State {
  idle: {
    kind: "idle";
  },

  live: {
    kind: "live";
    parameters: Parameters;
  },

  experiment: {
    kind: "experiment";
    prompt: ExperimentPrompt;
  }
}

@discriminated(#{ envelope: "none", discriminatorPropertyName: "experiment_type" })
union ExperimentAnswer {
  choice: {
    experiment_type: "choice";
  },

  rating: {
    experiment_type: "rating";
    value: integer;
  },
}

model InternalServerError is Response<500>;

@route("/state")
@tag("state")
@useAuth(BearerAuth)
interface StateRoutes {
  @route("/current")
  @summary("Get the current state")
  @get op getCurrentActivity(): State | InternalServerError;
  
  @route("/subscribe")
  @summary("Subscribe to state updates")
  @get op subscribeToActivity(): SSEStream<State> | InternalServerError;
}

@route("/experiment")
@tag("experiment")
@useAuth(BearerAuth)
interface ExperimentRoutes {
  @route("/answer")
  @summary("Answer the current experiment prompt")
  @post op answerExperiment(
    @body answer: ExperimentAnswer
  ): void | InternalServerError;

  @route("/swap")
  @summary("Swap between the two choices in the current experiment")
  @post op swapExperiment(): void | InternalServerError;
}
