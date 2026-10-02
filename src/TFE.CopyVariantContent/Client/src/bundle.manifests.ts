import { manifests as entityActions } from "./entity-action/manifests.js";
import { manifests as modals } from "./modal/manifests.js";

// The bundle collates every manifest exposed by the package. It is referenced from umbraco-package.json.
export const manifests: Array<UmbExtensionManifest> = [
  ...entityActions,
  ...modals,
];
