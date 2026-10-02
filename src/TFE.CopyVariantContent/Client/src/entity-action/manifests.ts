import { UMB_DOCUMENT_ENTITY_TYPE } from "@umbraco-cms/backoffice/document";

export const manifests: Array<UmbExtensionManifest> = [
  {
    type: "entityAction",
    kind: "default",
    alias: "Tfe.EntityAction.CopyVariantContent",
    name: "Copy Variant Content Entity Action",
    weight: 100,
    api: () => import("./copy-variant-content.action.js"),
    forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
    meta: {
      icon: "icon-globe",
      label: "Create variants",
    },
  },
];
