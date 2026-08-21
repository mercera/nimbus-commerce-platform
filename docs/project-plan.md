# Project Plan

Forward-looking roadmap for Nimbus Commerce Platform: what is being built, in what order, and what has
been deliberately deferred. This is the planning source — it is not a history and not an architecture
reference.

## Which document to use

| Question | Document |
|---|---|
| What are we building next, and in what order? | **`project-plan.md`** (this file) |
| What must the product do? What rules must hold? | `product-requirements.md` |
| How is the system built today, and why was it designed that way? | `Architecture.md` |
| What was actually built, when, and what did we learn? | `project-journal.md` |
| How do we work — workflow, review, traps to avoid? | `engineering-handbook.md` |
| How is code written — naming, patterns, conventions? | `coding-standards.md` |
| How do I build, run, migrate, configure? | `development-setup.md` |

A milestone is planned here, implemented, then recorded in `project-journal.md` and removed from
"upcoming" below. Product rules stay authoritative in `product-requirements.md` before and after
implementation. `Architecture.md` is updated only when the work changed the current architecture or
introduced a major system design decision — not for every completed item.

---

## Where the project stands

**Live:** Authentication end-to-end (register, login, refresh with rotation + reuse detection, logout,
`/me`), a React SPA exercising it, and the Product Catalogue's Category and Attribute Definition
management with per-category attribute configuration.

**Next:** Sprint 4 / M3 — Products core. Scope to be finalised before implementation.

**Blocked on nothing.** M2 delivered the category attribute configuration that Product attribute
validation is defined by.

---

## Phase overview

| Phase | Area | Status |
|---|---|---|
| Sprint 2 | Authentication (M1–M5) | ✅ Complete |
| Sprint 3 | Frontend foundation & authentication UI (M1) | ✅ Complete |
| Sprint 4 | **Product Catalogue** | 🔵 In progress |
| Later | Inventory, Orders, Customers | 💡 Direction only — not planned in detail |

---

## Sprint 4 — Product Catalogue roadmap

The catalogue was planned as one sequence before implementation began. Current state:

| # | Milestone | Status |
|---|---|---|
| 1 | Categories core | ✅ Sprint 4 / M1 — 2026-08-14 |
| 2 | Attribute Definitions | ✅ delivered within Sprint 4 / M2 — 2026-08-15 |
| 3 | Category ↔ AttributeDefinition configuration | ✅ delivered within Sprint 4 / M2 |
| 4 | **Products core** | 🔵 **Next — Sprint 4 / M3** |
| 5 | Product images | 📋 Planned |
| 6 | Frontend foundations | 📋 Planned |
| 7 | Frontend Products | 📋 Planned |
| 8 | Documentation | ♻️ Handled continuously per milestone — not a separate milestone |

**On the numbering:** steps 2 and 3 were originally planned as separate milestones and were delivered
together as Sprint 4 / M2. The next milestone is therefore **M3**, delivering what the original sequence
called step 4. Step 8 has been absorbed into the standard workflow — every milestone updates its own
documentation as it lands (see `engineering-handbook.md`).

---

## Next milestone — Sprint 4 / M3: Products core

**Goal.** Make `Product` and `ProductAttributeValue` behavioural: product CRUD and lifecycle, the
server-side list query, and attribute-value validation against the product's category configuration.

**Why now.** Product attribute validation cannot be built before the category configuration exists,
because "a valid attribute for this product" is *defined* by that configuration. M2 delivered it.

**Expected scope**

- `Product` aggregate behaviour: create, rename, change price, change SKU, move category, activate,
  deactivate, delete-where-allowed
- Attribute-value validation against the category's configuration — the four rules in
  `product-requirements.md` §7, all failures reported together
- Product store with the full list query: search by SKU/name, filter by category and status, sorting,
  database-level pagination
- Product endpoints, including a category picker endpoint (deferred from M2, where Products was
  identified as its first real consumer)
- Server-side rich-text sanitisation for the product description
- A deliberate seam for "has business history", which gates SKU immutability and hard delete — it has
  no real source until Inventory/Orders exist, and must be honest about that rather than guessing
- The two guards that were moot until now, because `ProductAttributeValue` could not have rows:
  blocking attribute-definition deletion when a Product holds a value, and blocking removal of an
  attribute from a Category while Products hold values for it

**Explicitly not in this milestone**

- Product images — milestone 5
- Any Product Catalogue frontend — milestones 6 and 7
- Filtering or searching by attribute value — out of scope for the catalogue entirely
- Product variants, price history, currency modelling, category nesting
- Optimistic concurrency, role seeding, rate limiting

**Depends on**

- *M1:* the catalogue schema and its constraints, the shared result and pagination types, the
  current-user accessor, the ownership-filtering rule, the unit test project
- *M2:* category attribute configuration as the source of truth for validation, and attribute
  definition active/inactive semantics

**Open planning question — scope shape.** M3 is currently held as **one milestone: Products core**.
A two-part split remains an available option if the milestone proves oversized once the file list is
drawn up:

- *M3a* — product CRUD, lifecycle and the list query, with no attribute values
- *M3b* — attribute validation, the attribute read/write paths, and the two guards above

The split is **not** an agreed decision. Its cost is that the product create/update contract would
change shape between the two parts; that is cheap today because no frontend consumes it yet. Decide
when M3's scope is finalised.

---

## Later catalogue milestones

Planned, not yet scoped in detail. Nothing below has an approved file list.

**Milestone 5 — Product images.** Image storage behind an abstraction with a local implementation
that mirrors the production model; signed, short-lived URLs for browser access; upload, delete,
set-primary and reorder; file-type validation by content rather than by extension or client-declared
type; the primary-image rules from `product-requirements.md` §8. Signed URLs were chosen because a
browser `<img src>` cannot carry a bearer token from an in-memory store and the refresh cookie is
scoped to the auth endpoints.

**Milestone 6 — Frontend foundations.** Adopt a server-state data-fetching library (TanStack Query v5
was agreed) with its cache-invalidation strategy; the shared UI primitives the catalogue screens need
(select, textarea, checkbox, data table, pagination, modal, confirm dialog, status badge, empty state,
inline loader); field-error handling lifted into the shared API layer; router and navigation updated.
Then the **Attribute Definitions** screen, then **Categories** including the attribute configurator.
Attribute definitions come first — the category configurator has nothing to pick from otherwise.

⚠️ **Carry-forward for whoever builds the configurator:** M2 delivered category attribute configuration
as discrete sub-resource operations (associate, toggle required, remove), *not* as a full-replacement
payload on category create/update, which is what the original plan assumed. Build against the
endpoints that exist.

**Milestone 7 — Frontend Products.** Product list with search, filters, sorting and pagination bound
to URL state; product detail; create/edit form; rich-text editor; an attribute editor driven by the
selected category's rules; a warning flow when changing category would discard incompatible values;
image management.

Two usability consequences to design for, both already identified: an attribute definition that is
configured on no category is invisible on every product form with nothing to explain why, so the
definition list should show how many categories use it; and deactivating a definition that is required
somewhere should warn that requiredness will be suspended while it is inactive.

---

## Decisions that shape future milestones

Product and domain rules are **not** repeated here — `product-requirements.md` is authoritative for
those, including the amendments covering per-category requiredness, requiredness suspension, deletion
guards and the `Price >= 0` rule. Architecture already implemented is not repeated here either —
see `Architecture.md`.

What remains below is only what constrains work not yet built:

- **Frontend server state uses TanStack Query v5**, with no client-state store. Agreed before any
  catalogue frontend work; it will be the frontend's fourth runtime dependency.
- **Product images use signed URLs**, not authenticated blob fetches. Reasoning above, milestone 5.
- **Rich-text description is sanitised server-side.** This is expected to introduce the catalogue's one
  third-party backend package — .NET ships no HTML sanitiser and hand-rolling one is a known
  anti-pattern. Confirm the package choice when M3's file list is approved.
- **"Has business history" is a seam, not a stored flag.** A denormalised boolean on `Product` would be
  faster and silently wrong the first time a future module forgot to set it. Future tables referencing
  `Products` must also declare no-cascade delete behaviour as a database backstop.
- **Attribute-value storage stays typed and structured**, leaving attribute-based filtering possible
  later without a schema change — even though that filtering is out of scope.

---

## Cross-cutting backlog

Carried forward across milestones and **not currently sequenced**. Each is tracked, accepted and
documented — none is an unknown defect. See `Architecture.md` → "Known limitations" for the full
reasoning behind those that are architectural.

| Item | Notes |
|---|---|
| Role seeding | Blocks every role-protected endpoint; roles are wired but no user can hold one |
| Seed data generally | No seeding of any kind exists — roles, demo catalogue, or otherwise |
| Rate limiting | `/register`, `/login`, `/refresh`, `/logout` are reachable and unthrottled |
| Email verification, password reset, MFA | Not started |
| Centralised `IsActive` enforcement | A deactivated user keeps working for up to the access-token lifetime on any endpoint that does not re-check |
| Optimistic concurrency (`RowVersion`) | No catalogue table has it; concurrent edits are last-write-wins |
| Parallel-refresh / logout-refresh race | Mitigated on the client by single-flight refresh; the server-side fix is deferred |
| Cross-tab refresh race | Single-tab case closed; cross-tab would need a browser lock primitive |
| CORS policy for non-proxied deployments | Development works via the Vite proxy; no deployment path exists without it |
| Frontend test project | None exists; the API client's retry/single-flight logic is the strongest first candidate |
| Refresh-token row cleanup | Revoked and expired rows accumulate with no cleanup job |
| Login timing side-channel | Partial mitigation only; full normalisation deferred |
| Per-device reuse-detection scope | Reuse detection revokes all of a user's sessions; narrowing it needs a schema change |
| Password-policy duplication | The server's real policy is stricter than the request annotation suggests; the frontend mirrors it by hand |

---

## Future ideas

Direction only. No design work has been done and nothing below should be treated as planned.

- **Inventory** — the next domain area in the `Product → Inventory → Order Item → Order` chain, and the
  first real source for "has business history".
- **Orders** and **Customers** — the reason the pagination envelope and result types were built to be
  reusable rather than product-specific.
- **Cart and Admin areas** — referenced as eventual application areas; no backend or frontend work has
  been done and neither has been scoped. The application shell's navigation shows Products and Orders
  as inert placeholders on purpose (see `engineering-handbook.md`).
- **Product variants** — deliberately deferred; expected to be revisited when Inventory or Orders
  create a concrete requirement.
- **Fine-grained roles** — Admin, Catalog Manager, Inventory Manager, Order Manager. Deferred until a
  concrete requirement exists; role seeding is the prerequisite either way.
- **Organisation-level multi-tenancy** — out of scope, but no current design may make it impossible.
