import { UmbModalToken } from "@umbraco-cms/backoffice/modal";

export const COPY_VARIANTS_MODAL_ALIAS = "Tfe.Modal.CopyVariants";

export interface CopyVariantsModalData {
  /** The name of the content item, shown in the confirmation text. */
  name?: string;
}

export interface CopyVariantsModalValue {
  /** Whether descendants should be processed as well. */
  includeChildren: boolean;
}

export const COPY_VARIANTS_MODAL = new UmbModalToken<CopyVariantsModalData, CopyVariantsModalValue>(
  COPY_VARIANTS_MODAL_ALIAS,
  {
    modal: {
      type: "dialog",
    },
  },
);
