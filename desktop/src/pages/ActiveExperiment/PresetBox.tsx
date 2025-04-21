import { CheckIcon } from "@heroicons/react/24/outline";
import classNames from "classnames";
import { commands, Preset } from "../../bindings.gen";
import Button from "../../components/Button";
import SliderCollection from "../../components/SliderCollection";
import { useCommand } from "../../hooks";
import styles from "./PresetBox.module.css";

interface PresetBoxProps {
  title: string;
  preset: Preset;
  active?: boolean;
  toggleable?: boolean;
  onClick?: () => void;
}

export function PresetBox(props: PresetBoxProps) {
  const { title, preset, active, toggleable } = props;

  const parameters = useCommand(commands.getParameters).data;

  const parameterStates = parameters.map((parameter) => ({
    ...parameter,
    value: preset.parameters[parameter.key],
  }));

  return (
    <div
      className={classNames(
        styles.container,
        active && styles.active,
        toggleable && styles.toggleable
      )}
      onClick={() => {
        if (toggleable && props.onClick) {
          props.onClick();
        }
      }}
    >
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>{title}</h1>
          <h1 className={styles.subtitle}>{preset.name}</h1>
        </div>

        {toggleable && active && (
          <Button square={true}>
            <CheckIcon className="icon" />
          </Button>
        )}
      </div>

      <SliderCollection parameters={parameterStates} />
    </div>
  );
}
