import { UMB_DOCUMENT_ENTITY_TYPE as a } from "@umbraco-cms/backoffice/document";
import { UmbModalToken as n } from "@umbraco-cms/backoffice/modal";
const o = [
  {
    type: "entityAction",
    kind: "default",
    alias: "Tfe.EntityAction.CopyVariantContent",
    name: "Copy Variant Content Entity Action",
    weight: 100,
    api: () => import("./copy-variant-content.action-CcuEllAS.js"),
    forEntityTypes: [a],
    meta: {
      icon: "icon-globe",
      label: "Create variants"
    }
  }
], t = "Tfe.Modal.CopyVariants", m = new n(
  t,
  {
    modal: {
      type: "dialog"
    }
  }
), e = [
  {
    type: "modal",
    alias: t,
    name: "Copy Variants Modal",
    element: () => import("./copy-variants-modal.element-BX5vSRIt.js")
  }
], p = [
  ...o,
  ...e
];
export {
  m as C,
  p as m
};
//# sourceMappingURL=bundle.manifests-DGvsXSQp.js.map
