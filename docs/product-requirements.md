# Product Requirements

Durable product and domain rules for Nimbus Commerce Platform — **what** the system must do and which
rules must hold, independent of how any of it is implemented.

This document is authoritative for product/domain rules and stays authoritative after a rule has been
implemented. It is not a roadmap and not a status report:

- **When** something gets built → `project-plan.md`
- **How** it was built, and when → `project-journal.md`
- **How** the current system realises a rule, and why that design was chosen → `Architecture.md`

Rules carry the reference numbers from the original Product Catalogue domain brief (e.g. **#10**) so
older planning notes and journal entries that cite those numbers remain traceable. Rules amended after
the original brief are marked **[amended]** and explained in "Amendments" at the end.

---

## 1. Scope

The Product Catalogue is the foundation that Inventory, Orders and Customers will later build on:

```
Product → Inventory → Order Item → Order
```

Only the Product Catalogue is in scope. A Product is the directly sellable unit — **product variants
are deferred** (#6) and are revisited during the Inventory/Order phases when a concrete requirement
exists, not before.

## 2. Catalogue ownership and isolation (#20)

- Every authenticated user owns an isolated catalogue of Products, Categories and Attribute Definitions.
- User A must never be able to access User B's catalogue, by any route.
- SKU uniqueness is scoped to the owner's catalogue.
- Attribute-definition name uniqueness is scoped to the owner's catalogue.
- Category name uniqueness is scoped to the owner's catalogue. **[amended]**
- Organisation-level multi-tenancy is deliberately not implemented, but no design choice may make a
  future move to organisation-owned catalogues impossible.

## 3. Product

### Identity and SKU (#1)
- Every Product has a required, user-defined SKU — user-controlled, never auto-generated.
- SKU is unique within the owner's catalogue.
- The database `Id` and the SKU are separate concepts: `Id` is the internal identifier, SKU is the
  business identifier.
- SKU is editable until the Product has business history, then immutable.

### Pricing (#2)
- A Product has exactly one current selling price, required, as a decimal monetary value.
- `Price >= 0`; `Price = 0` is valid, negative is not. **[amended]**
- Price may change over time. No price history, promotions, discounts, customer-specific pricing or
  price lists. Historical transaction pricing will belong to `OrderItem`, not `Product`.
- Currency is not modelled — a single implicit currency per deployment.

### Lifecycle (#3, #5)
- A Product is Active or Inactive. New Products default to Active.
- An Active Product must belong to an Active Category.
- An Inactive Product may be moved between categories, including into an Inactive Category.
- Product status is not inventory availability — an Active Product with zero inventory is valid.
- Inactive Products are retained rather than deleted.

### Description (#7)
- A Product has an optional rich-text description supporting paragraphs, headings, bold/italic, lists
  and links.
- Rich-text content is **untrusted input** and must be sanitised server-side before storage. Arbitrary
  HTML must never be stored or rendered on trust.
- Images are a separate concept and are never part of the description system.

### Creation and editing (#15)
- Creating a Product requires SKU, Name, Price and Category. Description, images and attribute values
  are optional. Status is automatically Active.
- Editable: Name, Description, Price, Category (subject to lifecycle rules), Images, Attributes.
- Status changes only through explicit activate/deactivate operations, never as a field edit.

### Deletion (#9)
- A Product may be hard-deleted only when it has never been referenced by another part of the system
  (orders, inventory, other business references).
- Once a Product has business history it must be deactivated instead. Historical business data is
  preserved.

## 4. Category (#4, #5)

- Category is a real domain entity, never a plain string on Product.
- Each Product belongs to exactly one Category.
- A Category is Active or Inactive; new Categories default to Active.
- A Category cannot be deactivated while it contains any Active Products.
- Deactivating a Category must not cascade to its Products.
- A Category may only be deleted when it holds no Products of any status.
- Categories are flat — no nesting.

## 5. Attribute Definitions (#10, #11, #16)

Products carry custom attributes through reusable, user-created definitions rather than a wide Product
table of fixed columns:

```
Attribute Definition → (configured per Category) → Product Attribute Value
```

- A definition has a unique name within the owner's catalogue and a declared data type.
- Data types are limited to **Text, Number, Decimal, Boolean**. This list is not expanded without
  justification.
- The data type is immutable once the definition exists. **[amended]**
- Definitions are reusable across many Products and many Categories.
- A definition is Active or Inactive. Deactivation preserves existing Product values.
- An Inactive definition cannot be added to a Category, and cannot be added to a Product that does not
  already hold a value for it.
- A definition cannot be hard-deleted while it is in use — where "in use" means **either** a Category
  is configured with it **or** a Product holds a value for it. **[amended]**

## 6. Category attribute configuration **[amended]**

Requiredness is a property of the **relationship between a Category and an Attribute Definition**, not
of the definition itself. The same attribute may be required in one Category and optional in another —
`Color` required for *Chairs*, optional for *Laptops*.

- A Category is configured with a set of Attribute Definitions; each association carries its own
  `IsRequired` flag.
- The same definition may be configured on any number of Categories.
- A duplicate association between one Category and one definition is not possible.
- Adding an association requires the definition to be Active.
- Making an attribute required is permitted unconditionally — it is **not** rejected because existing
  Products in that Category lack the value. Requiredness is enforced only when a Product is written.
- Removing an attribute from a Category is rejected while any Product in that Category holds a value
  for it.

## 7. Product attribute values

A Product's attributes are validated against **its Category's configuration**. Four rules apply:

| Rule | Requirement |
|---|---|
| Allowed | Every submitted attribute must be configured on the Product's Category |
| Required | Every attribute required for the Category must have a value — enforced only while the definition is Active **[amended]** |
| Type match | Exactly one value is supplied per attribute and it matches the declared data type |
| Inactive | An Inactive definition may be submitted only if the Product already holds a value for it |

Additional rules:

- The same attribute may not appear twice in one submission.
- All validation failures are reported together, not one per request.
- A Product's Category and its attribute values are validated as a single unit, so no Product can ever
  hold a value its Category does not permit.
- Values are stored in a structured, typed model — **never an untyped `Dictionary<string,string>`** (#10).

## 8. Product images (#8)

- A Product may have zero or many images.
- If any image exists, exactly one is primary.
- The first uploaded image becomes primary automatically.
- The primary image can be changed by the user.
- Deleting the primary image automatically promotes another image to primary.
- Images have an explicit display order.
- **Image binaries are never stored in SQL Server.** The database stores metadata and references only;
  production may later use cloud object storage. Cloud infrastructure is not introduced merely to make
  the platform look cloud-native.

## 9. Listing, search and pagination (#12, #13, #14)

- Product listing supports server-side pagination, search by SKU and Name, filtering by Category and by
  Active/Inactive status, and sorting.
- Filtering and sorting are applied **before** pagination, and all of it happens at the database/query
  level. The catalogue must never be loaded into memory and paged in application or React code.
- The pagination response is a reusable shape — `items`, `page`, `pageSize`, `totalCount`, `totalPages` —
  intended for Products, Orders, Customers and Inventory alike.
- The server enforces a maximum page size.
- **Filtering or searching by custom attribute value is deliberately not supported** (#12). It may be
  reconsidered later, once there is a concrete requirement and the query/performance implications can be
  assessed.

## 10. Authorization and audit (#17, #18)

- Every Product, Category and Attribute Definition endpoint requires authentication. No catalogue
  endpoint is publicly accessible.
- Any authenticated user may manage their own catalogue. Fine-grained roles (Admin, Catalog Manager,
  Inventory Manager, Order Manager) are deliberately deferred until a concrete requirement exists.
- Products, Categories and Attribute Definitions track `CreatedAtUtc`, `CreatedByUserId`,
  `UpdatedAtUtc`, `UpdatedByUserId`, using UTC timestamps and the authenticated user's id — never
  duplicated names or email addresses.
- This is audit metadata only. A full audit-history/event-sourcing system is out of scope.

## 11. Explicitly out of scope

Deliberately excluded from the Product Catalogue. Each is a decision, not an oversight:

Product variants · Inventory management · Orders · Customers · Price history · Discounts and promotions ·
Customer-specific pricing · Advanced custom-attribute filtering · Product recommendations · AI
functionality · RabbitMQ events for Products · Redis caching · Full audit history / event sourcing ·
Fine-grained product roles · Organisation-level multi-tenancy · Category nesting · Currency modelling.

Technology is not added to make the project look cloud-native.

---

## Amendments

Changes made to the original domain brief after it was written. Each was agreed explicitly.

**A1 — Requiredness moved from the Attribute Definition to the Category association.**
The original brief (#16) made "required" a property of the definition itself. That would force an
attribute to be required for every Product that uses it — `RAM` required on office chairs. Requiredness
is now carried per `(Category, Attribute Definition)` pair. See §6, and `Architecture.md` → "Product
Catalogue" for how the model realises it.

**A2 — Requiredness is enforced only while the definition is Active.**
"Required" and "Inactive" are otherwise jointly unsatisfiable: a deactivated definition that is required
in a Category would make new Products in that Category impossible to create — the value would be
simultaneously mandatory and forbidden. Deactivation suspends requiredness; reactivation restores it.
Existing values are untouched throughout. This rule was derived by necessity rather than stated in the
brief.

**A3 — Attribute-definition data type is immutable from creation, not merely "once in use".**
The brief (#16) made the data type immutable once the definition was in use. Immutable from creation is
simpler and avoids a state where changing the type is legal but has no safe migration path for values
stored later.

**A4 — "In use" for deletion covers Category associations as well as Product values.**
The brief (#11) blocked deletion only while Products used the definition. A definition configured on
Categories with zero Product values is also in use — deleting it would silently reconfigure those
Categories as a side effect. Deletion is blocked on either condition.

**A5 — Making an attribute required does not retroactively invalidate existing Products.**
The configuration change is always permitted; requiredness is enforced at Product write time. A Product
may therefore exist that would fail validation if re-saved, and its next update must supply the value.
The alternative — rejecting the configuration change — would make a reasonable administrative action
impossible on a large catalogue with no bulk fix-up tooling.

**A6 — Removing an attribute from a Category is blocked while Products hold values for it.**
This preserves the invariant that no Product ever holds a value its Category does not permit.

**A7 — Category names are unique per owner; `Price >= 0`.**
Neither was specified in the original brief; both were resolved explicitly.
