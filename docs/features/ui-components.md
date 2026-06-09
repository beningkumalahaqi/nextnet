---
uid: features/ui-components
title: UI Components
description: Pre-built visual components — Button, Card, Input, Modal, Navigation, and more
---

# UI Components `v5.0` `stable`

NextNet V5 ships with a library of pre-built, themeable UI components. Each component is designed with accessibility, customization, and performance in mind. All 17 built-in components are stable, fully tested, and ready for production use.

## Available Components

| Component | Description | Status |
|-----------|-------------|--------|
| `Button` | Clickable action element | ✅ Stable |
| `Card` | Content container with header/body/footer | ✅ Stable |
| `Input` | Text input with validation styling | ✅ Stable |
| `Select` | Dropdown selector | ✅ Stable |
| `Checkbox` | Multi-select input | ✅ Stable |
| `Radio` | Single-select input group | ✅ Stable |
| `Toggle` | On/off switch | ✅ Stable |
| `Modal` | Dialog overlay | ✅ Stable |
| `Drawer` | Slide-out panel | ✅ Stable |
| `Toast` | Notification toast | ✅ Stable |
| `Badge` | Status indicator pill | ✅ Stable |
| `Table` | Data table with sorting | ✅ Stable |
| `Tabs` | Tabbed content switcher | ✅ Stable |
| `Accordion` | Expandable sections | ✅ Stable |
| `Nav` | Navigation menu | ✅ Stable |
| `Progress` | Progress/loading indicator | ✅ Stable |
| `Avatar` | User avatar with fallback | ✅ Stable |

## Usage

Components are rendered via static methods:

```csharp
// File: app/page.cs
public class HomePage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public async Task<IHtmlContent> Render()
    {
        return HtmlHelper.Fragment(
            // Button component
            Button.Render("Click Me", new ButtonOptions
            {
                Variant = ButtonVariant.Primary,
                Size = ButtonSize.Lg
            }),

            // Card component
            Card.Render(new CardOptions
            {
                Title = "Welcome",
                Content = HtmlHelper.Text("This is a card body."),
                Footer = Button.Render("Learn More", new ButtonOptions
                {
                    Variant = ButtonVariant.Secondary
                })
            })
        );
    }
}
```

> [!TIP]
> All components are available in the `NextNet.DesignSystem.Components` namespace.
> Import with `using NextNet.DesignSystem.Components;`

## Button

The primary call-to-action element.

```csharp
Button.Render("Submit", new ButtonOptions
{
    Variant = ButtonVariant.Primary,
    Size = ButtonSize.Md,
    Disabled = false,
    Type = ButtonType.Submit,
    Icon = IconNames.Check,
    IconPosition = IconPosition.Right
});

// Icon-only button
Button.Render(new ButtonOptions
{
    Variant = ButtonVariant.Ghost,
    Size = ButtonSize.Sm,
    Icon = IconNames.Trash,
    AriaLabel = "Delete item"
});
```

### Button Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Variant` | `ButtonVariant` | `Primary` | Visual style (`Primary`, `Secondary`, `Ghost`, `Danger`, `Link`) |
| `Size` | `ButtonSize` | `Md` | Size (`Sm`, `Md`, `Lg`) |
| `Disabled` | `boolean` | `false` | Disable the button |
| `Type` | `ButtonType` | `Button` | HTML type (`Button`, `Submit`, `Reset`) |
| `FullWidth` | `boolean` | `false` | Stretch to container width |
| `Icon` | `IconNames` | `None` | Optional icon |
| `IconPosition` | `IconPosition` | `Left` | Icon placement |
| `Loading` | `boolean` | `false` | Show loading spinner |
| `AriaLabel` | `string` | `""` | Accessibility label |

### Button Variants

```text
┌─────────────┐  ┌──────────────┐  ┌───────────┐  ┌────────────┐  ┌────────┐
│  Primary    │  │  Secondary   │  │   Ghost   │  │   Danger   │  │  Link  │
│  (filled)   │  │  (outlined)  │  │ (flat)    │  │  (red)     │  │ (text) │
└─────────────┘  └──────────────┘  └───────────┘  └────────────┘  └────────┘
```

### Rendered HTML

```html
<button class="nn-btn nn-btn-primary nn-btn-md" type="button">
  <span class="nn-btn-icon nn-btn-icon-left">
    <svg><!-- icon --></svg>
  </span>
  <span class="nn-btn-label">Submit</span>
</button>
```

## Card

A flexible content container.

```csharp
Card.Render(new CardOptions
{
    Title = "Getting Started",
    Subtitle = "Follow these steps to begin",
    Image = "/images/card-hero.jpg",
    ImageAlt = "Hero image",
    Content = HtmlHelper.Fragment(
        HtmlHelper.Element("p", content: HtmlHelper.Text("Card body content here."))
    ),
    Footer = HtmlHelper.Fragment(
        Button.Render("Action", new ButtonOptions { Variant = ButtonVariant.Primary }),
        Button.Render("Cancel", new ButtonOptions { Variant = ButtonVariant.Ghost })
    ),
    Variant = CardVariant.Elevated,
    Padding = PaddingSize.Lg
});
```

### Card Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Title` | `string` | `""` | Card heading |
| `Subtitle` | `string` | `""` | Secondary text below title |
| `Image` | `string` | `""` | Hero image URL |
| `ImageAlt` | `string` | `""` | Image alt text |
| `Content` | `IHtmlContent` | `null` | Main body content |
| `Footer` | `IHtmlContent` | `null` | Footer content |
| `Variant` | `CardVariant` | `Elevated` | `Elevated`, `Outlined`, `Flat`, `Interactive` |
| `Padding` | `PaddingSize` | `Md` | Inner padding |
| `Href` | `string` | `""` | Makes card clickable (link) |

## Input

Text input with integrated validation and label.

```csharp
Input.Render(new InputOptions
{
    Name = "email",
    Label = "Email Address",
    Placeholder = "you@example.com",
    Type = InputType.Email,
    Required = true,
    Error = validationErrors.ContainsKey("email")
        ? validationErrors["email"]
        : "",
    Disabled = false,
    Size = InputSize.Md
});
```

### Input Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Name` | `string` | `""` | Form field name |
| `Label` | `string` | `""` | Display label |
| `Placeholder` | `string` | `""` | Placeholder text |
| `Type` | `InputType` | `Text` | HTML input type (`Text`, `Email`, `Password`, `Number`, `Tel`, `Url`, `Search`) |
| `Value` | `string` | `""` | Default value |
| `Required` | `boolean` | `false` | Mark as required |
| `Error` | `string` | `""` | Validation error message |
| `Disabled` | `boolean` | `false` | Disable input |
| `ReadOnly` | `boolean` | `false` | Read-only state |
| `Size` | `InputSize` | `Md` | `Sm`, `Md`, `Lg` |
| `Prefix` | `string` | `""` | Leading text/icon |
| `Suffix` | `string` | `""` | Trailing text/icon |
| `MaxLength` | `int` | `0` | Max character count |

### Input States

```csharp
// Default
Input.Render(new InputOptions { Name = "name", Label = "Name" });

// With error
Input.Render(new InputOptions
{
    Name = "email",
    Label = "Email",
    Error = "Please enter a valid email address"
});

// Disabled
Input.Render(new InputOptions
{
    Name = "disabled-field",
    Label = "Read Only",
    Disabled = true,
    Value = "Pre-filled content"
});

// With prefix/suffix
Input.Render(new InputOptions
{
    Name = "price",
    Label = "Price",
    Prefix = "$",
    Suffix = "USD",
    Type = InputType.Number
});
```

## Modal

Dialog overlay for confirmations, forms, and detail views.

```csharp
Modal.Render(new ModalOptions
{
    Id = "confirm-delete",
    Title = "Delete Item",
    Content = HtmlHelper.Text("Are you sure you want to delete this item? This action cannot be undone."),
    Footer = HtmlHelper.Fragment(
        Button.Render("Cancel", new ButtonOptions { Variant = ButtonVariant.Ghost }),
        Button.Render("Delete", new ButtonOptions { Variant = ButtonVariant.Danger })
    ),
    Size = ModalSize.Md,
    CloseOnBackdrop = true,
    ShowCloseButton = true
});
```

### Modal Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Id` | `string` | `""` | Unique ID for targeting |
| `Title` | `string` | `""` | Modal heading |
| `Content` | `IHtmlContent` | `null` | Body content |
| `Footer` | `IHtmlContent` | `null` | Footer actions |
| `Size` | `ModalSize` | `Md` | `Sm`, `Md`, `Lg`, `Xl`, `Fullscreen` |
| `CloseOnBackdrop` | `boolean` | `true` | Close when clicking outside |
| `ShowCloseButton` | `boolean` | `true` | Show X close button |
| `PreventScroll` | `boolean` | `true` | Lock body scroll when open |

## Table

Data table with sorting, headers, and customizable rows.

```csharp
Table.Render(new TableOptions
{
    Headers = new[] { "Name", "Email", "Role", "Status" },
    Rows = users.Select(u => new TableRow
    {
        Cells = new IHtmlContent[]
        {
            HtmlHelper.Text(u.Name),
            HtmlHelper.Text(u.Email),
            HtmlHelper.Text(u.Role),
            Badge.Render(u.Status, new BadgeOptions
            {
                Variant = u.Status == "Active"
                    ? BadgeVariant.Success
                    : BadgeVariant.Secondary
            })
        }
    }).ToArray(),
    Sortable = true,
    Striped = true,
    Hoverable = true,
    Dense = false
});
```

## Select

Dropdown selection component.

```csharp
Select.Render(new SelectOptions
{
    Name = "country",
    Label = "Country",
    Options = new[]
    {
        new SelectOption { Value = "", Label = "Select a country", Disabled = true },
        new SelectOption { Value = "us", Label = "United States" },
        new SelectOption { Value = "ca", Label = "Canada" },
        new SelectOption { Value = "mx", Label = "Mexico" },
    },
    Value = "us",
    Required = true,
    Placeholder = "Choose..."
});
```

## Toast

Notification component for success, error, and info messages.

```csharp
Toast.Render(new ToastOptions
{
    Message = "User saved successfully!",
    Type = ToastType.Success,
    Duration = 5000,  // Auto-dismiss after 5s
    Position = ToastPosition.TopRight,
    Dismissible = true
});

Toast.Render(new ToastOptions
{
    Message = "Failed to save changes.",
    Type = ToastType.Error,
    Duration = 0,  // Manual dismiss only
    Dismissible = true
});
```

## Tabs

Tabbed content switcher.

```csharp
Tabs.Render(new TabsOptions
{
    Tabs = new[]
    {
        new Tab { Id = "profile", Label = "Profile", Content = profileContent },
        new Tab { Id = "settings", Label = "Settings", Content = settingsContent },
        new Tab { Id = "billing", Label = "Billing", Content = billingContent },
    },
    ActiveTab = "profile",
    Variant = TabsVariant.Underlined  // or TabsVariant.Pills, TabsVariant.Enclosed
});
```

## Badge

Status indicator pill.

```csharp
Badge.Render("Active", new BadgeOptions
{
    Variant = BadgeVariant.Success  // Success, Warning, Error, Info, Neutral
});

Badge.Render("3", new BadgeOptions
{
    Variant = BadgeVariant.Danger,
    Size = BadgeSize.Sm  // Sm for count badges
});
```

## Accordion

Expandable sections for FAQs, settings, or grouped content.

```csharp
Accordion.Render(new AccordionOptions
{
    Items = new[]
    {
        new AccordionItem
        {
            Title = "Section 1",
            Content = HtmlHelper.Text("Content for section 1."),
            Expanded = true
        },
        new AccordionItem
        {
            Title = "Section 2",
            Content = HtmlHelper.Text("Content for section 2.")
        },
    },
    AllowMultiple = false  // Only one open at a time
});
```

## Customizing Components

### Via CSS Variables

All components use CSS custom properties for customization:

```css
:root {
  /* Override button styles */
  --nn-btn-primary-bg: #7c3aed;
  --nn-btn-primary-hover-bg: #6d28d9;
  --nn-btn-border-radius: 0.5rem;

  /* Override card styles */
  --nn-card-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.15);
  --nn-card-radius: 0.75rem;
}
```

### Via C# Options

Every component accepts an options object. Use inheritance or wrapper methods for reusable configurations:

```csharp
public static class MyButton
{
    public static IHtmlContent Danger(string text)
    {
        return Button.Render(text, new ButtonOptions
        {
            Variant = ButtonVariant.Danger,
            Size = ButtonSize.Md
        });
    }
}

// Usage
MyButton.Danger("Delete Account");
```

## Component Registration

If you only need a subset of components, configure in `nextnet.config.json`:

```json
{
  "designSystem": {
    "components": {
      "include": ["Button", "Card", "Input", "Badge"],
      "exclude": ["Modal", "Drawer", "Toast"]
    }
  }
}
```

This reduces the generated CSS and JavaScript to only what you use.

## Related

- **Concept**: [Design System](../core-concepts/design-system.md)
- **Feature**: [Theming](theming.md)
- **Feature**: [Tailwind Integration](tailwind-integration.md)
- **Reference**: [CLI Reference](../reference/cli-reference.md)
