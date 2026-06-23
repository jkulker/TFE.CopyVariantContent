import { COPY_VARIANTS_API_PATH } from "../constants.js";

export interface CreateVariantsResult {
  variantsCreated?: number;
  error?: unknown;
}

/**
 * Calls the package backoffice API to create the missing language variants.
 *
 * The Management API is authenticated with the current backoffice token, which the
 * caller resolves from the auth context (it also handles refresh).
 */
export async function createContentVariants(
  token: string,
  id: string,
  includeChildren: boolean,
): Promise<CreateVariantsResult> {
  try {
    const response = await fetch(COPY_VARIANTS_API_PATH, {
      method: "POST",
      credentials: "include",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ id, includeChildren }),
    });

    if (!response.ok) {
      return { error: `Request failed with status ${response.status}` };
    }

    const data = await response.json();
    return { variantsCreated: data?.variantsCreated ?? 0 };
  } catch (error) {
    return { error };
  }
}
