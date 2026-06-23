import { UmbEntityActionBase as c, UmbRequestReloadStructureForEntityEvent as u, UmbRequestReloadChildrenOfEntityEvent as d } from "@umbraco-cms/backoffice/entity-action";
import { UMB_AUTH_CONTEXT as C } from "@umbraco-cms/backoffice/auth";
import { umbOpenModal as p } from "@umbraco-cms/backoffice/modal";
import { UMB_NOTIFICATION_CONTEXT as h } from "@umbraco-cms/backoffice/notification";
import { UMB_ACTION_EVENT_CONTEXT as l } from "@umbraco-cms/backoffice/action";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT as T } from "@umbraco-cms/backoffice/document";
import { C as m } from "./bundle.manifests-DGvsXSQp.js";
const y = "/umbraco/copyvariantcontent/api/v1/create-variants";
async function f(n, e, a) {
  try {
    const t = await fetch(y, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${n}`
      },
      body: JSON.stringify({ id: e, includeChildren: a })
    });
    return t.ok ? { variantsCreated: (await t.json())?.variantsCreated ?? 0 } : { error: `Request failed with status ${t.status}` };
  } catch (t) {
    return { error: t };
  }
}
class N extends c {
  constructor(e, a) {
    super(e, a);
  }
  async execute() {
    if (!this.args.unique)
      return;
    const e = await p(this, m).catch(() => {
    });
    if (!e)
      return;
    const a = await this.getContext(h), t = await this.getContext(C);
    if (!t) {
      a?.peek("danger", {
        data: { headline: "Copy variants", message: "Not authenticated." }
      });
      return;
    }
    const i = await t.getLatestToken(), r = await f(i, this.args.unique, e.includeChildren);
    if (r.error) {
      a?.peek("danger", {
        data: {
          headline: "Copy variants",
          message: "Could not create the language variants."
        }
      });
      return;
    }
    a?.peek("positive", {
      data: {
        headline: "Copy variants",
        message: `Created ${r.variantsCreated ?? 0} variant(s).`
      }
    });
    const o = await this.getContext(T).catch(
      () => {
      }
    );
    if (o?.getUnique?.() === this.args.unique)
      await o?.reload();
    else {
      window.location.reload();
      return;
    }
    const s = await this.getContext(l).catch(() => {
    });
    s?.dispatchEvent(
      new u({
        unique: this.args.unique,
        entityType: this.args.entityType
      })
    ), e.includeChildren && s?.dispatchEvent(
      new d({
        unique: this.args.unique,
        entityType: this.args.entityType
      })
    );
  }
}
export {
  N as CopyVariantContentEntityAction,
  N as default
};
//# sourceMappingURL=copy-variant-content.action-CcuEllAS.js.map
