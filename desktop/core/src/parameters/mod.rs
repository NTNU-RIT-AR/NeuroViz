use crate::define_parameters;

mod parameter_macro;

define_parameters! {
  Transparency => {
      key: transparency,
      name: "Transparency",
      min: 0.0,
      max: 1.0,
      default: 0.0
  },
  Glow => {
      key: glow,
      name: "Glow",
      min: 0.0,
      max: 1.0,
      default: 0.0
  },
  Smoothness => {
      key: smoothness,
      name: "Smoothness",
      min: 0.0,
      max: 1.0,
      default: 0.5
  },
  Emission => {
      key: emission,
      name: "Emission",
      min: 0.0,
      max: 1.0,
      default: 0.0
  },
  LightIntensity => {
      key: light_intensity,
      name: "Light Intensity",
      min: 0.0,
      max: 2.0,
      default: 1.0
  },
  LightTemperature => {
      key: light_temperature,
      name: "Light Temperature",
      min: 1500.0,
      max: 20000.0,
      default: 6500.0
  },
}

#[cfg(test)]
mod tests {
    use std::collections::HashMap;

    use super::*;

    /// Tests that all parameters are listed in Parameter::all()
    #[test]
    fn test_all_is_correct() {
        let mut all_parameter_values = HashMap::new();

        for param in Parameter::all() {
            let json_key = serde_json::to_string(&param.key).unwrap();
            let unquoted_key = json_key.trim_matches('"');

            all_parameter_values.insert(unquoted_key.to_owned(), param.default);
        }

        // Serialize HashMap to json and serialize back as ParameterValues
        let all_parameter_values: ParameterValues =
            serde_json::from_str(&serde_json::to_string(&all_parameter_values).unwrap()).unwrap();
        let correct_parameter_values = ParameterValues::default();

        assert_eq!(all_parameter_values, correct_parameter_values);
    }
}
