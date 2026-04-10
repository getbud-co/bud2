"use client";

import { useEffect, useRef, useState, type ChangeEvent } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
  Input,
  Select,
  FilterDropdown,
} from "@mdonangelo/bud-ds";
import { CaretDown, Envelope } from "@phosphor-icons/react";
import { LANGUAGE_OPTIONS } from "../consts";

export interface InviteFormData {
  fullName: string;
  nickname: string;
  email: string;
  jobTitle: string;
  team: string;
  role: string;
  language: string;
}

interface RoleOption {
  value: string;
  label: string;
  description: string;
}

interface InviteUserModalProps {
  open: boolean;
  teamOptions: { value: string; label: string }[];
  roleOptions: RoleOption[];
  defaultRole: string;
  roleLabelBySlug: Map<string, string>;
  onClose: () => void;
  onSubmit: (data: InviteFormData) => void;
}

export function InviteUserModal({
  open,
  teamOptions,
  roleOptions,
  defaultRole,
  roleLabelBySlug,
  onClose,
  onSubmit,
}: InviteUserModalProps) {
  const [fullName, setFullName] = useState("");
  const [nickname, setNickname] = useState("");
  const [email, setEmail] = useState("");
  const [jobTitle, setJobTitle] = useState("");
  const [team, setTeam] = useState("");
  const [role, setRole] = useState(defaultRole);
  const [roleOpen, setRoleOpen] = useState(false);
  const roleBtnRef = useRef<HTMLButtonElement>(null);
  const [language, setLanguage] = useState("pt-br");

  useEffect(() => {
    if (!role && defaultRole) setRole(defaultRole);
  }, [role, defaultRole]);

  function handleSubmit() {
    onSubmit({
      fullName,
      nickname,
      email,
      jobTitle,
      team,
      role,
      language,
    });
    setFullName("");
    setNickname("");
    setEmail("");
    setJobTitle("");
    setTeam("");
    setRole(defaultRole);
    setLanguage("pt-br");
  }

  return (
    <Modal open={open} onClose={onClose} size="md">
      <ModalHeader title="Convidar usuário" onClose={onClose} />
      <ModalBody>
        <div className="flex flex-col gap-[var(--sp-md)]">
          <Input
            label="Nome completo"
            value={fullName}
            onChange={(e: ChangeEvent<HTMLInputElement>) =>
              setFullName(e.target.value)
            }
            placeholder="Nome Sobrenome"
          />
          <div className="grid grid-cols-2 gap-[var(--sp-md)]">
            <Input
              label="Apelido"
              value={nickname}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                setNickname(e.target.value)
              }
              placeholder="Como prefere ser chamado"
            />
            <Input
              label="E-mail"
              value={email}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                setEmail(e.target.value)
              }
              placeholder="email@empresa.com"
              leftIcon={Envelope}
            />
          </div>
          <div className="grid grid-cols-2 gap-[var(--sp-md)]">
            <Input
              label="Cargo"
              value={jobTitle}
              onChange={(e: ChangeEvent<HTMLInputElement>) =>
                setJobTitle(e.target.value)
              }
              placeholder="Ex: Product Manager"
            />
            <Select
              label="Time"
              value={team}
              onChange={setTeam}
              options={teamOptions}
            />
          </div>
          <Select
            label="Idioma"
            value={language}
            onChange={setLanguage}
            options={LANGUAGE_OPTIONS}
          />
          <div className="flex flex-col gap-[var(--sp-3xs)] relative">
            <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-700)] leading-[1.15]">
              Tipo de usuário
            </span>
            <button
              ref={roleBtnRef}
              className="flex items-center gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] cursor-pointer min-h-[36px] text-left hover:border-[var(--color-caramel-500)] focus-visible:outline-2 focus-visible:outline-[var(--color-orange-500)] focus-visible:outline-offset-2"
              onClick={() => setRoleOpen((v) => !v)}
              type="button"
            >
              <span className="flex-1 min-w-0 font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] leading-[1.15]">
                {roleLabelBySlug.get(role) ?? "Selecione"}
              </span>
              <CaretDown
                size={14}
                className="shrink-0 text-[var(--color-neutral-400)] transition-transform duration-150"
              />
            </button>
            <FilterDropdown
              open={roleOpen}
              onClose={() => setRoleOpen(false)}
              anchorRef={roleBtnRef}
            >
              <div className="flex flex-col p-[var(--sp-3xs)] max-h-[360px] overflow-y-auto">
                {roleOptions.map((opt) => (
                  <button
                    key={opt.value}
                    type="button"
                    className={`flex items-start gap-[var(--sp-2xs)] px-[var(--sp-sm)] py-[var(--sp-2xs)] border-0 bg-transparent cursor-pointer text-left w-full rounded-[var(--radius-xs)] transition-colors duration-[120ms] hover:bg-[var(--color-caramel-100)] ${role === opt.value ? "bg-[var(--color-caramel-50)]" : ""}`}
                    onClick={() => {
                      setRole(opt.value);
                      setRoleOpen(false);
                    }}
                  >
                    <div className="flex flex-col gap-[2px] flex-1 min-w-0">
                      <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)] leading-[1.15]">
                        {opt.label}
                      </span>
                      <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)] leading-[1.35]">
                        {opt.description}
                      </span>
                    </div>
                  </button>
                ))}
              </div>
            </FilterDropdown>
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
          disabled={!fullName.trim() || !email.trim()}
          onClick={handleSubmit}
        >
          Enviar convite
        </Button>
      </ModalFooter>
    </Modal>
  );
}
