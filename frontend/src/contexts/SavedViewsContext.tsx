"use client";

import {
  createContext,
  useContext,
  useState,
  useCallback,
  useMemo,
  type ReactNode,
} from "react";

export interface CalendarDate {
  year: number;
  month: number;
  day: number;
}

export interface SavedViewFilters {
  activeFilters: string[];
  selectedTeams: string[];
  selectedPeriod: [CalendarDate | null, CalendarDate | null];
  selectedStatus: string;
  selectedOwners: string[];
  selectedType?: string;
  selectedCategory?: string;
  selectedItemTypes?: string[];
  selectedIndicatorTypes?: string[];
  selectedContributions?: string[];
  selectedTaskState?: string;
  selectedMissionStatuses?: string[];
  selectedSupporters?: string[];
}

export interface SavedView {
  id: string;
  name: string;
  module: string;
  filters: SavedViewFilters;
}

interface SavedViewsContextValue {
  views: SavedView[];
  addView: (view: Omit<SavedView, "id">) => string;
  updateView: (id: string, updates: Partial<Omit<SavedView, "id">>) => void;
  deleteView: (id: string) => void;
}

const SavedViewsContext = createContext<SavedViewsContextValue>({
  views: [],
  addView: () => "",
  updateView: () => {},
  deleteView: () => {},
});

export function SavedViewsProvider({ children }: { children: ReactNode }) {
  const [views, setViews] = useState<SavedView[]>([]);

  const addView = useCallback((view: Omit<SavedView, "id">) => {
    const id = `view-${Date.now()}`;
    setViews((prev) => [...prev, { ...view, id }]);
    return id;
  }, []);

  const updateView = useCallback(
    (id: string, updates: Partial<Omit<SavedView, "id">>) => {
      setViews((prev) =>
        prev.map((v) => (v.id === id ? { ...v, ...updates } : v)),
      );
    },
    [],
  );

  const deleteView = useCallback((id: string) => {
    setViews((prev) => prev.filter((v) => v.id !== id));
  }, []);

  const value = useMemo(
    () => ({ views, addView, updateView, deleteView }),
    [views, addView, updateView, deleteView],
  );

  return (
    <SavedViewsContext.Provider value={value}>
      {children}
    </SavedViewsContext.Provider>
  );
}

export function useSavedViews() {
  return useContext(SavedViewsContext);
}
