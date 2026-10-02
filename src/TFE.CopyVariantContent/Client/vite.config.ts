import { defineConfig } from "vite";

export default defineConfig({
  build: {
    lib: {
      entry: "src/bundle.manifests.ts", // Bundle registers the package manifests
      formats: ["es"],
      fileName: "copy-variant-content",
    },
    outDir: "../wwwroot/App_Plugins/CopyVariantContent", // built bundle is served from here
    emptyOutDir: true,
    sourcemap: true,
    rollupOptions: {
      external: [/^@umbraco/],
    },
  },
});
