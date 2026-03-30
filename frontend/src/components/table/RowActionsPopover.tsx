"use client";

import { useRef } from "react";
import { Button, Popover } from "@mdonangelo/bud-ds";
import type { PopoverItem } from "@mdonangelo/bud-ds";
import { DotsThreeVertical } from "@phosphor-icons/react";

interface RowActionsPopoverProps {
  items: PopoverItem[];
  open: boolean;
  onToggle: () => void;
  onClose: () => void;
  className?: string;
  buttonAriaLabel?: string;
}

export function RowActionsPopover({
  items,
  open,
  onToggle,
  onClose,
  className,
  buttonAriaLabel = "Abrir ações",
}: RowActionsPopoverProps) {
  const triggerRef = useRef<HTMLButtonElement | null>(null);

  return (
    <div className={className}>
      <Button
        ref={triggerRef}
        variant="secondary"
        size="md"
        leftIcon={DotsThreeVertical}
        aria-label={buttonAriaLabel}
        onClick={onToggle}
      />
      <Popover
        items={items}
        open={open}
        onClose={onClose}
        anchorRef={triggerRef}
      />
    </div>
  );
}
