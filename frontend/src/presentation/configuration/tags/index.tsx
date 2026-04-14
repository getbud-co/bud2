import { useMemo, useState } from "react";
import { formatDateBR } from "@/lib/tempStorage/date-format";
import {
  Table,
  TableCardHeader,
  TableContent,
  TableHead,
  TableBody,
  TableRow,
  TableHeaderCell,
  TableCell,
  TableBulkActions,
  Button,
  Input,
  Badge,
  toast,
  RowActionsPopover,
  useDataTable,
} from "@getbud-co/buds";
import type { PopoverItem } from "@getbud-co/buds";
import {
  Plus,
  PencilSimple,
  Trash,
  Tag as TagIcon,
  MagnifyingGlass,
} from "@phosphor-icons/react";
import { useConfigData } from "@/contexts/ConfigDataContext";
import type { TagView } from "./types";
import { TagFormModal } from "./components/TagFormModal";
import { DeleteTagModal } from "./components/DeleteTagModal";
import { PageHeader } from "@/presentation/layout/page-header";

export function TagsModule() {
  const { tags, createTag, updateTag, deleteTag } = useConfigData();
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingTag, setEditingTag] = useState<TagView | null>(null);
  const [deletingTag, setDeletingTag] = useState<TagView | null>(null);
  const [actionsPopoverTag, setActionsPopoverTag] = useState<string | null>(
    null,
  );

  type SortKey = "name" | "linkedItems" | "createdAt";
  const {
    selectedRows,
    clearSelection,
    sortKey,
    sortDir,
    handleSort,
    getSortDirection,
    handleSelectRow,
    handleSelectAll,
  } = useDataTable<SortKey>();

  function parseDate(str: string): number {
    const [d = 1, m = 1, y = 1970] = str.split("/").map(Number);
    return new Date(y, m - 1, d).getTime();
  }

  const tagsView = useMemo<TagView[]>(
    () =>
      tags.map((tag) => ({
        ...tag,
        linkedItems: 0,
        createdAt: formatDateBR(tag.createdAt),
        updatedAt: formatDateBR(tag.updatedAt),
      })),
    [tags],
  );

  const filtered = useMemo(
    () =>
      tagsView
        .filter(
          (t) => !search || t.name.toLowerCase().includes(search.toLowerCase()),
        )
        .sort((a, b) => {
          if (!sortKey) return 0;
          const dir = sortDir === "asc" ? 1 : -1;
          switch (sortKey) {
            case "name":
              return dir * a.name.localeCompare(b.name);
            case "linkedItems":
              return dir * (a.linkedItems - b.linkedItems);
            case "createdAt":
              return dir * (parseDate(a.createdAt) - parseDate(b.createdAt));
            default:
              return 0;
          }
        }),
    [tagsView, search, sortKey, sortDir],
  );

  const rowIds = useMemo(() => filtered.map((t) => t.id), [filtered]);

  function handleBulkDelete() {
    for (const tagId of selectedRows) {
      deleteTag(tagId);
    }
    toast.success(`${selectedRows.size} tag(s) excluída(s)`);
    clearSelection();
  }

  function openCreate() {
    setEditingTag(null);
    setModalOpen(true);
  }

  function openEdit(tag: TagView) {
    setEditingTag(tag);
    setModalOpen(true);
  }

  function handleSave(name: string, color: string) {
    if (editingTag) {
      updateTag(editingTag.id, { name, color });
      toast.success("Tag atualizada");
    } else {
      createTag({ name, color });
      toast.success("Tag criada");
    }
    setModalOpen(false);
  }

  function handleDelete() {
    if (!deletingTag) return;
    deleteTag(deletingTag.id);
    setDeletingTag(null);
    toast.success("Tag excluída");
  }

  function getRowActions(tag: TagView): PopoverItem[] {
    return [
      {
        id: "edit",
        label: "Editar",
        icon: PencilSimple,
        onClick: () => openEdit(tag),
      },
      {
        id: "delete",
        label: "Excluir",
        icon: Trash,
        danger: true,
        onClick: () => setDeletingTag(tag),
      },
    ];
  }

  return (
    <div className="flex flex-col gap-[var(--sp-2xs)] w-full">
      <PageHeader title="Tags e organizadores" />
      <div className="flex flex-col gap-[var(--sp-2xs)] min-w-0">
        <Table
          variant="divider"
          elevated={false}
          selectable
          selectedRows={selectedRows}
          rowIds={rowIds}
          onSelectRow={handleSelectRow}
          onSelectAll={(checked: boolean) => handleSelectAll(checked, rowIds)}
        >
          <TableCardHeader
            title="Tags"
            badge={<Badge color="neutral">{filtered.length}</Badge>}
            actions={
              <div className="flex items-center gap-[var(--sp-2xs)]">
                <div className="max-w-[400px] flex-1">
                  <Input
                    placeholder="Buscar tag..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    leftIcon={MagnifyingGlass}
                  />
                </div>
                <Button
                  variant="primary"
                  size="md"
                  leftIcon={Plus}
                  onClick={openCreate}
                >
                  Nova tag
                </Button>
              </div>
            }
          />
          <TableContent>
            <TableHead>
              <TableRow>
                <TableHeaderCell isCheckbox />
                <TableHeaderCell
                  sortable
                  sortDirection={getSortDirection("name")}
                  onSort={() => handleSort("name")}
                >
                  Nome
                </TableHeaderCell>
                <TableHeaderCell
                  sortable
                  sortDirection={getSortDirection("linkedItems")}
                  onSort={() => handleSort("linkedItems")}
                >
                  Itens vinculados
                </TableHeaderCell>
                <TableHeaderCell
                  sortable
                  sortDirection={getSortDirection("createdAt")}
                  onSort={() => handleSort("createdAt")}
                >
                  Criado em
                </TableHeaderCell>
                <TableHeaderCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((t) => (
                <TableRow key={t.id} rowId={t.id}>
                  <TableCell isCheckbox rowId={t.id} />
                  <TableCell>
                    <Badge color={t.color as "neutral"} leftIcon={TagIcon}>
                      {t.name}
                    </Badge>
                  </TableCell>
                  <TableCell>{t.linkedItems}</TableCell>
                  <TableCell>{t.createdAt}</TableCell>
                  <TableCell>
                    <RowActionsPopover
                      className="flex justify-end"
                      items={getRowActions(t)}
                      open={actionsPopoverTag === t.id}
                      onToggle={() =>
                        setActionsPopoverTag(
                          actionsPopoverTag === t.id ? null : t.id,
                        )
                      }
                      onClose={() => setActionsPopoverTag(null)}
                      buttonAriaLabel={`Abrir ações da tag ${t.name}`}
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </TableContent>
          <TableBulkActions count={selectedRows.size} onClear={clearSelection}>
            <Button
              variant="danger"
              size="md"
              leftIcon={Trash}
              onClick={handleBulkDelete}
            >
              Excluir
            </Button>
          </TableBulkActions>
        </Table>

        <TagFormModal
          open={modalOpen}
          editingTag={editingTag}
          onClose={() => setModalOpen(false)}
          onSave={handleSave}
        />

        <DeleteTagModal
          tag={deletingTag}
          onClose={() => setDeletingTag(null)}
          onConfirm={handleDelete}
        />
      </div>
    </div>
  );
}
