"use client";

import { useRef, useState } from "react";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from "@getbud-co/buds";
import { DownloadSimple, FileText, UploadSimple } from "@phosphor-icons/react";
import { DEFAULT_ROLE_SLUG, GENDER_OPTIONS, LANGUAGE_OPTIONS } from "../consts";

interface ImportUsersModalProps {
  open: boolean;
  teamOptions: { value: string; label: string }[];
  roleOptions: { value: string; label: string }[];
  onClose: () => void;
  onSubmit: (file: File) => void;
}

export function ImportUsersModal({
  open,
  teamOptions,
  roleOptions,
  onClose,
  onSubmit,
}: ImportUsersModalProps) {
  const [importFile, setImportFile] = useState<File | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  function handleClose() {
    setImportFile(null);
    onClose();
  }

  function handleSubmit() {
    if (!importFile) return;
    onSubmit(importFile);
    setImportFile(null);
  }

  function handleDownloadTemplate() {
    const headers = [
      "Nome",
      "Sobrenome",
      "Apelido",
      "E-mail",
      "Cargo",
      "Time",
      "Tipo de usuário",
      "Data de nascimento",
      "Idioma",
      "Gênero",
    ];
    const exampleRow = [
      "Maria",
      "Soares",
      "Mari",
      "maria@empresa.com",
      "Product Manager",
      "Produto; Liderança",
      DEFAULT_ROLE_SLUG,
      "15/03/1990",
      "pt-br",
      "Feminino",
    ];
    const hintsRow = [
      "",
      "",
      "",
      "",
      "",
      `Múltiplos times separados por ; — Valores: ${teamOptions.map((t) => t.value).join(" | ")}`,
      `Valores: ${roleOptions.map((r) => r.value).join(" | ")}`,
      "Formato: DD/MM/AAAA",
      `Valores: ${LANGUAGE_OPTIONS.map((l) => l.value).join(" | ")}`,
      `Valores: ${GENDER_OPTIONS.map((g) => g.label).join(" | ")}`,
    ];

    const escape = (v: string) =>
      v.includes(",") || v.includes('"') ? `"${v.replace(/"/g, '""')}"` : v;
    const csv = [headers, exampleRow, hintsRow]
      .map((row) => row.map(escape).join(","))
      .join("\n");
    const bom = "\uFEFF";
    const blob = new Blob([bom + csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "template-usuarios-bud.csv";
    a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <Modal open={open} onClose={handleClose} size="sm">
      <ModalHeader title="Importar usuários" onClose={handleClose} />
      <ModalBody>
        <div className="flex flex-col gap-[var(--sp-md)]">
          <p className="font-[var(--font-body)] text-[var(--text-sm)] text-[var(--color-neutral-600)] m-0 leading-[1.5]">
            Faça o upload de uma planilha com os dados dos usuários para
            cadastrá-los em massa na plataforma.
          </p>
          <button
            type="button"
            className="flex items-center gap-[var(--sp-sm)] p-[var(--sp-sm)] border border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-neutral-0)] cursor-pointer text-left w-full transition-colors duration-[120ms] text-[var(--color-green-600)] hover:bg-[var(--color-caramel-50)]"
            onClick={handleDownloadTemplate}
          >
            <FileText size={20} />
            <div className="flex flex-col gap-[2px] flex-1 min-w-0">
              <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-900)]">
                Baixar template
              </span>
              <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                Template CSV com os campos e valores aceitos
              </span>
            </div>
            <DownloadSimple size={16} />
          </button>
          <div
            className="flex flex-col items-center justify-center gap-[var(--sp-2xs)] p-[var(--sp-xl)] border-2 border-dashed border-[var(--color-caramel-300)] rounded-[var(--radius-sm)] bg-[var(--color-caramel-50)] cursor-pointer transition-[border-color,background-color] duration-[120ms] text-[var(--color-neutral-400)] hover:border-[var(--color-orange-300)] hover:bg-[var(--color-orange-50)]"
            onClick={() => fileInputRef.current?.click()}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept=".xls,.xlsx,.csv"
              className="hidden"
              onChange={(e) => setImportFile(e.target.files?.[0] ?? null)}
            />
            <UploadSimple size={24} />
            {importFile ? (
              <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-orange-600)]">
                {importFile.name}
              </span>
            ) : (
              <>
                <span className="font-[var(--font-label)] font-medium text-[var(--text-sm)] text-[var(--color-neutral-700)]">
                  Arraste ou clique para selecionar
                </span>
                <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-500)]">
                  .xls, .xlsx ou .csv
                </span>
              </>
            )}
          </div>
        </div>
      </ModalBody>
      <ModalFooter>
        <Button variant="tertiary" size="md" onClick={handleClose}>
          Cancelar
        </Button>
        <Button
          variant="primary"
          size="md"
          disabled={!importFile}
          onClick={handleSubmit}
        >
          Importar
        </Button>
      </ModalFooter>
    </Modal>
  );
}
