"use client";

import { Input, Button, toast } from "@mdonangelo/bud-ds";
import { UploadSimpleIcon } from "@phosphor-icons/react";
import { useOrganization } from "@/contexts/OrganizationContext";
import { useActiveOrganization } from "../hooks/useActiveOrganization";
import { ConfigLoadingState } from "@/components/ConfigLoadingState";
import { ConfigErrorState } from "@/components/ConfigErrorState";

function formatCnpj(value: string): string {
  const digits = value.replace(/\D/g, "").slice(0, 14);
  if (digits.length <= 2) return digits;
  if (digits.length <= 5) return `${digits.slice(0, 2)}.${digits.slice(2)}`;
  if (digits.length <= 8)
    return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5)}`;
  if (digits.length <= 12)
    return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8)}`;
  return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8, 12)}-${digits.slice(12)}`;
}

export function CompanyInfoTab() {
  const { activeOrgId } = useOrganization();
  const { data: org, isLoading, isError } = useActiveOrganization(activeOrgId);

  function handleLogoUpload() {
    const input = document.createElement("input");
    input.type = "file";
    input.accept = "image/png, image/jpeg, image/svg+xml";
    input.onchange = (e) => {
      const file = (e.target as HTMLInputElement).files?.[0];
      if (file) {
        toast.success("Símbolo atualizado");
      }
    };
    input.click();
  }

  if (isLoading) return <ConfigLoadingState />;
  if (isError || !org)
    return (
      <ConfigErrorState message="Não foi possível carregar os dados da empresa. Verifique sua conexão e tente novamente." />
    );

  return (
    <div className="flex flex-col gap-[var(--sp-md)] p-[var(--sp-lg)]">
      <div className="font-[var(--font-heading)] text-[var(--text-md)] font-semibold text-[var(--color-neutral-900)]">
        Símbolo da empresa
      </div>

      <div className="flex items-center gap-[var(--sp-md)] max-md:flex-col max-md:items-start">
        <div className="shrink-0">
          {org.logoUrl ? (
            <img
              src={org.logoUrl}
              alt="Símbolo da empresa"
              className="w-16 h-16 rounded-[var(--radius-sm)] object-cover border border-[var(--color-caramel-200)]"
            />
          ) : (
            <div className="w-16 h-16 rounded-[var(--radius-sm)] border-2 border-dashed border-[var(--color-caramel-300)] flex items-center justify-center text-[var(--color-neutral-400)] bg-[var(--color-caramel-50)]">
              <UploadSimpleIcon size={24} />
            </div>
          )}
        </div>
        <div className="flex flex-col gap-[var(--sp-3xs)]">
          <Button
            variant="secondary"
            size="sm"
            leftIcon={UploadSimpleIcon}
            onClick={handleLogoUpload}
          >
            {org.logoUrl ? "Alterar símbolo" : "Enviar símbolo"}
          </Button>
          <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)]">
            Imagem quadrada, PNG, JPG ou SVG. Máx. 2MB.
          </span>
          <span className="font-[var(--font-body)] text-[var(--text-xs)] text-[var(--color-neutral-400)]">
            Este símbolo será exibido no seletor de organizações da barra
            lateral.
          </span>
        </div>
      </div>

      {org.logoUrl && (
        <div className="flex items-center gap-[var(--sp-sm)]">
          <span className="font-[var(--font-label)] text-[var(--text-xs)] text-[var(--color-neutral-500)] whitespace-nowrap">
            Prévia no seletor:
          </span>
          <div className="flex items-center gap-[var(--sp-2xs)] px-[var(--sp-xs)] py-[var(--sp-2xs)] border border-[var(--color-caramel-200)] rounded-[var(--radius-sm)] bg-white">
            <img
              src={org.logoUrl}
              alt=""
              className="w-6 h-6 rounded-[var(--radius-2xs)] object-cover"
            />
            <span className="font-[var(--font-label)] font-medium text-[var(--text-xs)] text-[var(--color-neutral-950)]">
              {org.name}
            </span>
          </div>
        </div>
      )}

      <div className="font-[var(--font-heading)] text-[var(--text-md)] font-semibold text-[var(--color-neutral-900)]">
        Informações básicas
      </div>

      <div className="grid grid-cols-2 gap-[var(--sp-md)] max-md:grid-cols-1">
        <Input
          label="Nome da empresa"
          disabled={true}
          value={org.name}
          placeholder="Nome fantasia ou razão social"
        />
        <Input
          label="CNPJ"
          disabled={true}
          value={formatCnpj(org.cnpj)}
          placeholder="00.000.000/0000-00"
        />
      </div>
    </div>
  );
}
