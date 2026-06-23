import { UmbEntityActionBase as s, UmbRequestReloadStructureForEntityEvent as c, UmbRequestReloadChildrenOfEntityEvent as u } from "@umbraco-cms/backoffice/entity-action";
import { UMB_AUTH_CONTEXT as d } from "@umbraco-cms/backoffice/auth";
import { umbOpenModal as C } from "@umbraco-cms/backoffice/modal";
import { UMB_NOTIFICATION_CONTEXT as p } from "@umbraco-cms/backoffice/notification";
import { UMB_ACTION_EVENT_CONTEXT as h } from "@umbraco-cms/backoffice/action";
import { UMB_DOCUMENT_WORKSPACE_CONTEXT as T } from "@umbraco-cms/backoffice/document";
import { C as m } from "./bundle.manifests-CHvn06P7.js";
const y = "/umbraco/copyvariantcontent/api/v1/create-variants";
async function l(n, e, a) {
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
class N extends s {
  constructor(e, a) {
    super(e, a);
  }
  async execute() {
    if (!this.args.unique)
      return;
    const e = await C(this, m).catch(() => {
    });
    if (!e)
      return;
    const a = await this.getContext(p), t = await this.getContext(d);
    if (!t) {
      a?.peek("danger", {
        data: { headline: "Copy variants", message: "Not authenticated." }
      });
      return;
    }
    const i = await t.getLatestToken(), r = await l(i, this.args.unique, e.includeChildren);
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
    }), await (await this.getContext(T).catch(
      () => {
      }
    ))?.reload();
    const o = await this.getContext(h).catch(() => {
    });
    o?.dispatchEvent(
      new c({
        unique: this.args.unique,
        entityType: this.args.entityType
      })
    ), e.includeChildren && o?.dispatchEvent(
      new u({
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
//# sourceMappingURL=copy-variant-content.action-Cc_O8x2d.js.map
