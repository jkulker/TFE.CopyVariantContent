import { COPY_VARIANTS_MODAL_ALIAS } from "./copy-variants-modal.token.js";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "modal",
    alias: COPY_VARIANTS_MODAL_ALIAS,
    name: "Copy Variants Modal",
    element: () => import("./copy-variants-modal.element.js"),
  },
];
