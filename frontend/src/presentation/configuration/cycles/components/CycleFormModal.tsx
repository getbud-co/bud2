"use client";

import { useState, useEffect } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  Input,
  Select,
  DatePicker,
  ChoiceBox,
  ChoiceBoxGroup,
} from "@getbud-co/buds";
import type { CalendarDate } from "@getbud-co/buds";
import type { Cycle, CycleCadence, CycleStatus } from "@/types";
import { calendarDateToIso, isoToCalendarDate } from "../utils";
import { TYPE_OPTIONS } from "../consts";

export interface CycleFormData {
  name: string;
  cadence: CycleCadence;
  startDate: string;
  endDate: string;
  status: CycleStatus;
}

interface CycleFormModalProps {
  open: boolean;
  editingCycle: Cycle | null;
  onClose: () => void;
  onSave: (data: CycleFormData) => void;
  isPending: boolean;
}

export function CycleFormModal({
  open,
  editingCycle,
  onClose,
  onSave,
  isPending,
}: CycleFormModalProps) {
  const [name, setName] = useState("");
  const [cadence, setCadence] = useState<CycleCadence>("Quarterly");
  const [dates, setDates] = useState<
    [CalendarDate | null, CalendarDate | null]
  >([null, null]);
  const [status, setStatus] = useState<CycleStatus>("Planning");

  useEffect(() => {
    if (!open) return;
    if (editingCycle) {
      setName(editingCycle.name);
      setCadence(editingCycle.cadence);
      setDates([
        isoToCalendarDate(editingCycle.startDate),
        isoToCalendarDate(editingCycle.endDate),
      ]);
      setStatus(editingCycle.status);
    } else {
      setName("");
      setCadence("Quarterly");
      setDates([null, null]);
      setStatus("Planning");
    }
  }, [open, editingCycle]);

  function handleSave() {
    if (!dates[0] || !dates[1]) return;
    onSave({
      name,
      cadence,
      startDate: calendarDateToIso(dates[0]),
      endDate: calendarDateToIso(dates[1]),
      status,
    });
  }

  return (
    <Modal open={open} onClose={onClose} size="md">
      <ModalHeader
        title={editingCycle ? "Editar ciclo" : "Novo ciclo"}
        onClose={onClose}
      />
      <ModalBody>
        <div className="flex flex-col gap-[var(--sp-md)]">
          <Input
            label="Nome do ciclo"
            value={name}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setName(e.target.value)
            }
            placeholder="Ex: Q1 2027"
          />
          <Select
            label="Tipo"
            value={cadence}
            onChange={(value: string) => setCadence(value as CycleCadence)}
            options={TYPE_OPTIONS}
          />
          <div>
            <label className="block [font-family:var(--font-label)] [font-size:var(--text-sm)] text-[var(--color-neutral-700)] mb-[var(--sp-2xs)]">
              Período
            </label>
            <DatePicker mode="range" value={dates} onChange={setDates} />
          </div>
          <div>
            <label className="block [font-family:var(--font-label)] [font-size:var(--text-sm)] text-[var(--color-neutral-700)] mb-[var(--sp-2xs)]">
              Status
            </label>
            <ChoiceBoxGroup
              value={status}
              onChange={(v) => setStatus(v as CycleStatus)}
            >
              <ChoiceBox
                value="Active"
                title="Ativo"
                description="O ciclo está em andamento e visível para os usuários"
              />
              <ChoiceBox
                value="Planning"
                title="Futuro"
                description="Agendado para início posterior, ainda não disponível"
              />
              <ChoiceBox
                value="Ended"
                title="Encerrado"
                description="O ciclo já foi finalizado e está disponível apenas para consulta"
              />
            </ChoiceBoxGroup>
          </div>
        </div>
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={onClose}>
          Cancelar
        </Button>
        <Button
          variant="primary"
          size="md"
          disabled={!name.trim() || !dates[0] || !dates[1] || isPending}
          onClick={handleSave}
        >
          {editingCycle ? "Salvar" : "Criar ciclo"}
        </Button>
      </ModalFooter>
    </Modal>
  );
}
