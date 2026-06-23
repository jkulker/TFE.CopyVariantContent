import { UmbEntityActionBase } from "@umbraco-cms/backoffice/entity-action";
import type { UmbEntityActionArgs } from "@umbraco-cms/backoffice/entity-action";
import type { UmbControllerHost } from "@umbraco-cms/backoffice/controller-api";
import { UMB_AUTH_CONTEXT } from "@umbraco-cms/backoffice/auth";
import { umbOpenModal } from "@umbraco-cms/backoffice/modal";
import { UMB_NOTIFICATION_CONTEXT } from "@umbraco-cms/backoffice/notification";
import { UMB_ACTION_EVENT_CONTEXT } from "@umbraco-cms/backoffice/action";
import {
  UmbRequestReloadStructureForEntityEvent,
  UmbRequestReloadChildrenOfEntityEvent,
} from "@umbraco-cms/backoffice/entity-action";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from "@umbraco-cms/backoffice/document";
import { COPY_VARIANTS_MODAL } from "../modal/copy-variants-modal.token.js";
import { createContentVariants } from "../repository/copy-variant-content.server.data-source.js";

export class CopyVariantContentEntityAction extends UmbEntityActionBase<never> {
  constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
    super(host, args);
  }

  override async execute() {
    if (!this.args.unique) {
      return;
    }

    // umbOpenModal rejects when the editor cancels the dialog; treat that as "do nothing".
    const value = await umbOpenModal(this, COPY_VARIANTS_MODAL).catch(() => undefined);
    if (!value) {
      return;
    }

    const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
    const authContext = await this.getContext(UMB_AUTH_CONTEXT);
    if (!authContext) {
      notificationContext?.peek("danger", {
        data: { headline: "Copy variants", message: "Not authenticated." },
      });
      return;
    }

    const token = await authContext.getLatestToken();
    const result = await createContentVariants(token, this.args.unique, value.includeChildren);

    if (result.error) {
      notificationContext?.peek("danger", {
        data: {
          headline: "Copy variants",
          message: "Could not create the language variants.",
        },
      });
      return;
    }

    notificationContext?.peek("positive", {
      data: {
        headline: "Copy variants",
        message: `Created ${result.variantsCreated ?? 0} variant(s).`,
      },
    });

    // The document workspace context only listens to recycle-bin events, not to
    // RequestReloadStructureForEntityEvent. So to refresh the open variant
    // selector we must call reload() on the workspace itself. The event dispatch
    // afterwards is for the tree / collections / info-apps that DO listen.
    const workspaceContext = await this.getContext(UMB_DOCUMENT_WORKSPACE_CONTEXT).catch(
      () => undefined,
    );
    const workspaceUnique = workspaceContext?.getUnique?.();
    if (workspaceUnique === this.args.unique) {
      await workspaceContext?.reload();
    } else {
      // Triggered from the tree drawer with no (or a different) document open in
      // the workspace; the variant selector for the targeted document can't be
      // reloaded in place, so refresh the page to pick up the new variants.
      window.location.reload();
      return;
    }

    const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT).catch(() => undefined);
    eventContext?.dispatchEvent(
      new UmbRequestReloadStructureForEntityEvent({
        unique: this.args.unique,
        entityType: this.args.entityType,
      }),
    );
    if (value.includeChildren) {
      eventContext?.dispatchEvent(
        new UmbRequestReloadChildrenOfEntityEvent({
          unique: this.args.unique,
          entityType: this.args.entityType,
        }),
      );
    }
  }
}

export default CopyVariantContentEntityAction;
