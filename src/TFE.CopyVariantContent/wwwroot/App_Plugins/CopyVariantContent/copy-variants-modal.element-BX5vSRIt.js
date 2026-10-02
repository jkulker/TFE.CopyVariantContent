import { html as d, css as f, state as v, customElement as C } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement as y } from "@umbraco-cms/backoffice/modal";
import { UmbTextStyles as g } from "@umbraco-cms/backoffice/style";
var b = Object.defineProperty, w = Object.getOwnPropertyDescriptor, c = (e) => {
  throw TypeError(e);
}, h = (e, t, a, l) => {
  for (var n = l > 1 ? void 0 : l ? w(t, a) : t, r = e.length - 1, s; r >= 0; r--)
    (s = e[r]) && (n = (l ? s(t, a, n) : s(n)) || n);
  return l && n && b(t, a, n), n;
}, x = (e, t, a) => t.has(e) || c("Cannot " + a), E = (e, t, a) => t.has(e) ? c("Cannot add the same private member more than once") : t instanceof WeakSet ? t.add(e) : t.set(e, a), u = (e, t, a) => (x(e, t, "access private method"), a), i, m, p, _;
let o = class extends y {
  constructor() {
    super(...arguments), E(this, i), this._includeChildren = !1;
  }
  render() {
    return d`
      <uui-dialog-layout headline="Create variants">
        <p>
          Copy
          ${this.data?.name ? d`<strong>${this.data.name}</strong>` : "this content"}
          into every language variant that has not been created yet?
        </p>

        <uui-form-layout-item>
          <uui-toggle
            label="Include all items below"
            ?checked=${this._includeChildren}
            @change=${u(this, i, m)}
          ></uui-toggle>
        </uui-form-layout-item>

        <div slot="actions">
          <uui-button label="Cancel" @click=${u(this, i, _)}></uui-button>
          <uui-button
            look="primary"
            color="positive"
            label="Create content for all variants"
            @click=${u(this, i, p)}
          ></uui-button>
        </div>
      </uui-dialog-layout>
    `;
  }
};
i = /* @__PURE__ */ new WeakSet();
m = function() {
  this._includeChildren = !this._includeChildren;
};
p = function() {
  this.value = { includeChildren: this._includeChildren }, this.modalContext?.submit();
};
_ = function() {
  this.modalContext?.reject();
};
o.styles = [
  g,
  f`
      :host {
        display: block;
        min-width: 400px;
        max-width: 90vw;
      }
    `
];
h([
  v()
], o.prototype, "_includeChildren", 2);
o = h([
  C("tfe-copy-variants-modal")
], o);
const O = o;
export {
  o as TfeCopyVariantsModalElement,
  O as default
};
//# sourceMappingURL=copy-variants-modal.element-BX5vSRIt.js.map
