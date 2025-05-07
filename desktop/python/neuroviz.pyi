from typing import Optional, TypedDict

class ParameterDict(TypedDict):
    """Dictionary containing visualization parameters"""
    transparency: float
    glow: float
    smoothness: float
    emission: float
    light_intensity: float
    light_temperature: float

class NeuroViz:
    """
    NeuroViz class that maintains a HTTP server for neural visualization.

    Attributes:
        ip: The local IP address the server is running on
        port: The port the server is listening on
        secret: Optional security token used for authentication
    """
    ip: str
    port: int
    secret: Optional[str]

    def __init__(self, port: int, use_secret: bool) -> None:
        """
        Creates a new NeuroViz instance which starts an HTTP server for visualization.

        Args:
            port: The port to run the HTTP server on
            use_secret: Whether to generate a secret token for secure connections

        The server will run until the instance is garbage collected.
        """
        ...

    def set_live_parameters(self, parameters: ParameterDict) -> None:
        """
        Update the visualization parameters.

        Args:
            parameters: Dictionary containing parameter keys and values.
        """
        ...

    def prompt_choice(self, a: ParameterDict, b: ParameterDict) -> ParameterDict:
        """
        Prompt the user with a choice between two parameter sets.

        Args:
            a: First parameter set for comparison
            b: Second parameter set for comparison

        Returns:
            The parameter set that was chosen (either a or b)
        """
        ...

    def prompt_rating(self, parameters: ParameterDict) -> int:
        """
        Prompt the user to rate the given parameter set.

        Args:
            parameters: The parameter set to be rated

        Returns:
            An integer rating value between 1 and 5
        """
        ...
        
    def set_idle(self) -> None:
        """
        Set the visualization state to idle.
        """
        ...

def default_parameters() -> ParameterDict:
    """
    Returns the default parameter values.
    
    Returns:
        A ParameterDict with default visualization parameters.
    """
    ...
