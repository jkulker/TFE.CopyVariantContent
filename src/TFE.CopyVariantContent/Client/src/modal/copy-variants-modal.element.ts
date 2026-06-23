import { css, customElement, html, state } from "@umbraco-cms/backoffice/external/lit";
import { UmbModalBaseElement } from "@umbraco-cms/backoffice/modal";
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import type { CopyVariantsModalData, CopyVariantsModalValue } from "./copy-variants-modal.token.js";

@customElement("tfe-copy-variants-modal")
export class TfeCopyVariantsModalElement extends UmbModalBaseElement<
  CopyVariantsModalData,
  CopyVariantsModalValue
> {
  @state()
  private _includeChildren = false;

  #onToggleIncludeChildren() {
    this._includeChildren = !this._includeChildren;
  }

  #submit() {
    this.value = { includeChildren: this._includeChildren };
    this.modalContext?.submit();
  }

  #close() {
    this.modalContext?.reject();
  }

  override render() {
    return html`
      <uui-dialog-layout headline="Create variants">
        <p>
          Copy
          ${this.data?.name ? html`<strong>${this.data.name}</strong>` : "this content"}
          into every language variant that has not been created yet?
        </p>

        <uui-form-layout-item>
          <uui-toggle
            label="Include all items below"
            ?checked=${this._includeChildren}
            @change=${this.#onToggleIncludeChildren}
          ></uui-toggle>
        </uui-form-layout-item>

        <div slot="actions">
          <uui-button label="Cancel" @click=${this.#close}></uui-button>
          <uui-button
            look="primary"
            color="positive"
            label="Create content for all variants"
            @click=${this.#submit}
          ></uui-button>
        </div>
      </uui-dialog-layout>
    `;
  }

  static override styles = [
    UmbTextStyles,
    css`
      :host {
        display: block;
        min-width: 400px;
        max-width: 90vw;
      }
    `,
  ];
}

export default TfeCopyVariantsModalElement;

declare global {
  interface HTMLElementTagNameMap {
    "tfe-copy-variants-modal": TfeCopyVariantsModalElement;
  }
}
