import SwaggerUI from "swagger-ui";
import SwaggerUIStandalonePreset from "swagger-ui/dist/swagger-ui-standalone-preset";
import "swagger-ui/dist/swagger-ui.css";
import spec from "../public/neuroviz.yaml";

SwaggerUI({
  spec: spec,
  urls: [
    {
      url: "./neuroviz.yaml",
      name: "Neuroviz",
    },
  ],
  dom_id: "#app",
  deepLinking: true,
  presets: [SwaggerUI.presets.apis, SwaggerUIStandalonePreset],
  layout: "StandaloneLayout",
});
