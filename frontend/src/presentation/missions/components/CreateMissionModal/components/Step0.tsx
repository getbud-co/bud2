import { ChoiceBoxGroup, ChoiceBox } from "@getbud-co/buds";
import { MISSION_TEMPLATES } from "../../../consts";

interface Step0Props {
  selectedTemplate: string | undefined;
  onChange: (t: string | undefined) => void;
}

export function Step0({ selectedTemplate, onChange }: Step0Props) {
  return (
    <div className="flex flex-col gap-[var(--sp-sm)]">
      <p className="m-0 [font-family:var(--font-heading)] font-semibold text-[var(--text-sm)] text-[var(--color-neutral-950)] leading-[1.25]">
        Escolha o seu template de missão
      </p>
      <ChoiceBoxGroup
        value={selectedTemplate}
        onChange={(v: string | undefined) => onChange(v)}
      >
        {MISSION_TEMPLATES.map((t) => (
          <ChoiceBox
            key={t.value}
            value={t.value}
            title={t.title}
            description={t.description}
          />
        ))}
      </ChoiceBoxGroup>
    </div>
  );
}
