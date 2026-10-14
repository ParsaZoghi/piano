# Graph Report - piano  (2026-10-13)

## Corpus Check
- 125 files · ~211,600 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 55 file(s) not represented in the graph (top: .csv 53, (none) 1, .css 1)

## Summary
- 1601 nodes · 2801 edges · 95 communities (75 shown, 20 thin omitted)
- Extraction: 90% EXTRACTED · 10% INFERRED · 0% AMBIGUOUS · INFERRED: 272 edges (avg confidence: 0.86)
- Token cost: 430,481 input · 0 output

## Community Hubs (Navigation)
- shadcn Component Installer
- UI/UX Search Core (BM25)
- Design Token Starter: Types
- Design Token Starter: Colors
- CIP & Logo Search Core
- Design Token Starter: Components
- Slide Search Engine
- Design Slides Copy & Strategy
- Design System CLI Formatting
- Design Token Starter: Scales
- Design System Generator
- HTML Token Validator
- Design Skill Routing
- Core Data Quality Tests
- Slide Deck Generator
- Tailwind Config Init Tests
- Three-Layer Token Architecture
- Data Contract Validator
- Dark Mode Resolution
- Logo Prompt & Style Guide
- Icon Generator
- Slide Background Fetcher
- Brand Asset & Logo Rules
- Relevance Evaluator Tests
- Brand Skill & AI Generation
- Slides Copywriting Formulas
- shadcn Accessibility & Components
- Color Contrast & Accessibility
- Social Photos Design
- CIP Style Guide
- Style Taxonomy & Catalog Tests
- Web Stack Freshness Tests
- Banner Export & Asset Approval
- CIP Deliverables
- Cross-File Data Contract Tests
- CIP Image Generator
- Design Token Starter: Spacing
- Native/Desktop Stack Freshness Tests
- Brand Color Extractor
- Token Sync & Validation Tests
- Tailwind Breakpoints
- Palette Luminance Checks
- Catalog Refresh Tests
- Banner Sizes & Art Direction
- Brand Asset Validator
- Tailwind Config Generator Class
- Reasoning Rule Contract
- CIP HTML Renderer
- Logo Generator
- Tailwind Fonts & Spacing
- Dark Palette Derivation
- Domain Detection Tests
- Search Domain Tests
- Brand Voice & Messaging
- Text Layout Resilience Tests
- Token Embedder
- Token Validator Script
- Landing & Stack Contracts
- Piano package.json (Vite)
- Component Specifications
- UX Rule Categories & Forms
- Brand Context Injector
- Design Token Starter: Durations
- Design Banner Sizes (duplicate)
- Slides HTML Template
- Brand-to-Tokens Sync
- Token CSS Generator
- CIP Search CLI
- Reasoning & Stack Freshness Checks
- Catalog Contract Checks
- Logo Search CLI
- Slide Layout Patterns
- Core Diagnostics Tests
- Design Token Starter: Radius
- Design System Persistence
- Graphify Project Instructions
- Slide Token Validator (Legacy)
- shadcn & Tailwind Theming
- Icon Design Reference
- Responsive Breakpoints
- Canvas Design System
- Design Token Starter: Shadow sm
- Tokenizer Tests
- Slide Decision System
- Design Token Starter: lg
- BM25 Core Behavior Tests
- Design Token Starter: xl
- Design Token Starter: md
- Design Token Starter: none
- Piano App Entry

## God Nodes (most connected - your core abstractions)
1. `TailwindConfigGenerator` - 58 edges
2. `TestTailwindConfigGenerator` - 35 edges
3. `DesignSystemGenerator` - 35 edges
4. `ShadcnInstaller` - 34 edges
5. `Design Skill (SKILL.md)` - 34 edges
6. `TestShadcnInstaller` - 26 edges
7. `read_rows()` - 16 edges
8. `color` - 15 edges
9. `search()` - 15 edges
10. `CatalogRefreshTest` - 15 edges

## Surprising Connections (you probably didn't know these)
- `generate-tokens.cjs Script` --semantically_similar_to--> `sync-brand-to-tokens.cjs Script`  [INFERRED] [semantically similar]
  .claude/skills/design-system/SKILL.md → .claude/skills/brand/SKILL.md
- `.claude/CLAUDE.md (graphify skill registration)` --conceptually_related_to--> `CLAUDE.md (project instructions)`  [INFERRED]
  .claude/CLAUDE.md → CLAUDE.md
- `Asset Approval Checklist` --semantically_similar_to--> `Brand Consistency Checklist`  [INFERRED] [semantically similar]
  .claude/skills/brand/references/approval-checklist.md → .claude/skills/brand/references/consistency-checklist.md
- `Color System Hierarchy (Primary, Secondary, Neutral, Semantic)` --semantically_similar_to--> `Core Visual Elements (Logo, Color Palette, Typography)`  [INFERRED] [semantically similar]
  .claude/skills/brand/references/color-palette-management.md → .claude/skills/brand/references/visual-identity.md
- `State Accessibility Requirements (contrast, ARIA states)` --semantically_similar_to--> `WCAG 2.1 Contrast Ratios`  [INFERRED] [semantically similar]
  .claude/skills/design-system/references/states-and-variants.md → .claude/skills/brand/references/color-palette-management.md

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Brand Guidelines to Design Tokens Sync Pipeline** — _claude_skills_brand_skill_brand_guidelines_md, _claude_skills_brand_skill_sync_brand_to_tokens_cjs, _claude_skills_brand_skill_design_tokens_json, _claude_skills_brand_skill_design_tokens_css, _claude_skills_brand_skill_inject_brand_context_cjs [EXTRACTED 1.00]
- **Three-Layer Token System (Primitive, Semantic, Component)** — _claude_skills_design_system_references_primitive_tokens, _claude_skills_design_system_references_semantic_tokens, _claude_skills_design_system_references_component_tokens, _claude_skills_design_system_references_token_architecture [EXTRACTED 1.00]
- **Banner Production Skill Chain** — _claude_skills_banner_design_skill_ui_ux_pro_max_skill, _claude_skills_banner_design_skill_frontend_design_skill, _claude_skills_banner_design_skill_ai_artist_skill, _claude_skills_banner_design_skill_ai_multimodal_skill, _claude_skills_banner_design_skill_chrome_devtools_skill, _claude_skills_brand_skill_inject_brand_context_cjs [EXTRACTED 1.00]
- **Slides Knowledge Base (create, layouts, template, copywriting, strategies)** — _claude_skills_design_references_slides_create, _claude_skills_design_references_slides_layout_patterns, _claude_skills_design_references_slides_html_template, _claude_skills_design_references_slides_copywriting_formulas, _claude_skills_design_references_slides_strategies [EXTRACTED 1.00]
- **Complete Brand Package Flow (Logo to CIP to Presentation)** — _claude_skills_design_skill_logo_design, _claude_skills_design_skill_cip_design, _claude_skills_design_skill_slides [EXTRACTED 1.00]
- **Design System Skill Chain (brand, design-system, ui-styling)** — _claude_skills_design_skill_brand_skill, _claude_skills_design_skill_design_system_skill, _claude_skills_design_skill_ui_styling_skill [EXTRACTED 1.00]
- **Slides Skill Knowledge Base (strategy + layout + copy + template)** — _claude_skills_slides_references_slide_strategies_slide_strategies, _claude_skills_slides_references_layout_patterns_layout_patterns, _claude_skills_slides_references_copywriting_formulas_copywriting_formulas, _claude_skills_slides_references_html_template_html_slide_template, _claude_skills_slides_references_copywriting_formulas_search_slides_py [EXTRACTED 1.00]
- **Emotion-Driven Slide Design (emotion arc -> formula mapping -> layout decision)** — _claude_skills_slides_references_slide_strategies_emotion_arc, _claude_skills_slides_references_copywriting_formulas_formula_to_slide_mapping, _claude_skills_slides_references_layout_patterns_layout_decision_flow, _claude_skills_slides_references_slide_strategies_duarte_sparkline [INFERRED 0.85]
- **UI Styling Core Stack (component layer + styling layer + visual design layer)** — _claude_skills_ui_styling_skill_shadcn_ui, _claude_skills_ui_styling_skill_tailwind_css, _claude_skills_ui_styling_skill_canvas_visual_design_layer, _claude_skills_ui_styling_skill_radix_ui [EXTRACTED 1.00]

## Communities (95 total, 20 thin omitted)

### Community 0 - "shadcn Component Installer"
Cohesion: 0.05
Nodes (3): main(), ShadcnInstaller, TestShadcnInstaller

### Community 1 - "UI/UX Search Core (BM25)"
Cohesion: 0.06
Nodes (31): BM25, _contains_phrase(), detect_domain(), _domain_keywords(), _exact_match_diagnostic(), _exact_row_identity(), _exact_stack_identifier(), _file_signature() (+23 more)

### Community 2 - "Design Token Starter: Types"
Cohesion: 0.05
Nodes (53): $type, $value, $type, $value, $type, $value, $type, $value (+45 more)

### Community 3 - "Design Token Starter: Colors"
Cohesion: 0.04
Nodes (48): $type, $value, background, destructive, destructive-foreground, foreground, muted, muted-foreground (+40 more)

### Community 4 - "CIP & Logo Search Core"
Cohesion: 0.06
Nodes (13): BM25, detect_domain(), get_cip_brief(), _load_csv(), search(), search_all(), _search_csv(), BM25 (+5 more)

### Community 5 - "Design Token Starter: Components"
Cohesion: 0.06
Nodes (45): $type, $value, $type, $value, bg, fg, font-size, hover-bg (+37 more)

### Community 6 - "Slide Search Engine"
Cohesion: 0.08
Nodes (17): format_context(), format_result(), main(), BM25, calculate_pattern_break(), detect_domain(), get_background_config(), get_color_for_emotion() (+9 more)

### Community 7 - "Design Slides Copy & Strategy"
Cohesion: 0.12
Nodes (33): Slides Reference, Slides Copywriting Formulas, AIDA (Attention-Interest-Desire-Action), Before-After-Bridge, Cost of Inaction formula, FAB (Features-Advantages-Benefits), Formula-to-Slide Mapping, Headline Patterns (+25 more)

### Community 8 - "Design System CLI Formatting"
Cohesion: 0.08
Nodes (12): ansi_ljust(), _detect_page_type(), format_ascii_box(), format_master_md(), format_page_override_md(), _generate_intelligent_overrides(), hex_to_ansi(), persist_design_system() (+4 more)

### Community 9 - "Design Token Starter: Scales"
Cohesion: 0.06
Nodes (34): $type, $value, $type, $value, $type, $value, $type, $value (+26 more)

### Community 10 - "Design System Generator"
Cohesion: 0.11
Nodes (3): DesignSystemGenerator, _resolve_dial(), TestReasoningMatch

### Community 11 - "HTML Token Validator"
Cohesion: 0.12
Nodes (12): get_context(), is_allowed_exception(), is_allowed_rgba(), is_inside_block(), load_css_variables(), main(), print_result(), print_summary() (+4 more)

### Community 12 - "Design Skill Routing"
Cohesion: 0.19
Nodes (26): CIP Models (flash / pro), Design Routing Guide, Multi-Skill Workflows (setup, migration, component creation), Routing by Question Type, Routing by Task Type, When to Use Multiple Skills, Design Skill (SKILL.md), ai-multimodal skill (+18 more)

### Community 13 - "Core Data Quality Tests"
Cohesion: 0.15
Nodes (10): read_rows(), TestAccessibilityGuidance, TestChartsTypographyAndIcons, TestCurrentReactGuidance, TestSemanticColors, _check_typography_contract(), _configured_font_names(), _declared_weights() (+2 more)

### Community 14 - "Slide Deck Generator"
Cohesion: 0.13
Nodes (11): _e(), generate_chart_slide(), generate_cta_slide(), generate_deck(), generate_metrics_slide(), generate_problem_slide(), generate_solution_slide(), generate_testimonial_slide() (+3 more)

### Community 16 - "Three-Layer Token Architecture"
Cohesion: 0.12
Nodes (17): Dialog Spec, Table Spec, Component Tokens, Dialog/Modal Tokens (--dialog-*), Table Tokens (--table-*), Primitive Tokens, Semantic Tokens, Focus Ring Spec (+9 more)

### Community 17 - "Data Contract Validator"
Cohesion: 0.17
Nodes (14): _check_app_interface_contract(), _check_chart_contract(), _check_color_contract(), _check_core_data_contract(), _check_icon_contract(), _check_landing_claims(), _check_motion_contract(), _check_react_contract() (+6 more)

### Community 18 - "Dark Mode Resolution"
Cohesion: 0.14
Nodes (6): _filter_anti_patterns_for_mode(), _query_wants_dark(), _resolve_color_mode(), _style_is_dark_primary(), TestAntiPatternGating, TestModeResolution

### Community 19 - "Logo Prompt & Style Guide"
Cohesion: 0.15
Nodes (17): Core Identity (primary logo, variations), CIP Negative Prompts, Logo Design Reference, Logo Available Styles (55+), Logo Industry Defaults, Logo AI Prompt Engineering, Logo Core Prompt Structure, Effective Keywords by Style (+9 more)

### Community 20 - "Icon Generator"
Cohesion: 0.19
Nodes (8): apply_color(), apply_viewbox_size(), extract_svgs(), generate_batch(), generate_icon(), generate_sizes(), load_env(), main()

### Community 21 - "Slide Background Fetcher"
Cohesion: 0.16
Nodes (9): generate_css_for_background(), get_background_image(), get_curated_images(), get_overlay_css(), get_pexels_search_url(), load_backgrounds_config(), load_brand_colors(), main() (+1 more)

### Community 22 - "Brand Asset & Logo Rules"
Cohesion: 0.12
Nodes (18): Post-Approval Archival, Asset Directory Structure (.assets metadata + assets raw files), manifest.json Asset Registry, Asset Tagging System, Logo Usage Rules, Incorrect Logo Usage (Don'ts), Logo Variants (Full Horizontal, Stacked, Icon, Wordmark, Color Variants), Typography Specifications (+10 more)

### Community 23 - "Relevance Evaluator Tests"
Cohesion: 0.12
Nodes (3): TestFixtureValidation, TestMetricMath, TestThresholdGate

### Community 24 - "Brand Skill & AI Generation"
Cohesion: 0.18
Nodes (16): ai-artist Skill, ai-multimodal Skill, AI Visual Generation (Gemini Flash vs Pro image models), Brand Guidelines Template, Extractable Brand Fields (colors, typography, voice, logo), Brand Color Presets (ocean-professional, electric-creative, forest-calm, midnight-purple, sunset-warm), docs/brand-guidelines.md (Brand Source of Truth), assets/design-tokens.css (+8 more)

### Community 25 - "Slides Copywriting Formulas"
Cohesion: 0.17
Nodes (14): AIDA (Attention-Interest-Desire-Action), Before-After-Bridge (BAB), Copywriting Formulas (25 formulas for slide copy), Cost of Inaction Formula, FAB (Features-Advantages-Benefits), Headline Patterns (Power Words, Contrast, Social Proof), PAS (Problem-Agitate-Solution), search-slides.py (design-system slide knowledge search script) (+6 more)

### Community 26 - "shadcn Accessibility & Components"
Cohesion: 0.16
Nodes (17): Apache License 2.0 (ui-styling LICENSE.txt), Keyboard Navigation and Focus Management (focus-visible, skip link, dialog focus trap), Screen Reader Support (semantic HTML, ARIA labels), shadcn/ui Accessibility Patterns, Form & Input Components (Button, Input, Form, Select, Checkbox, Radio Group), shadcn/ui Component Reference, Layout Utilities (Display, Flexbox, Grid, Positioning, Z-Index), Tailwind CSS Utility Reference (+9 more)

### Community 27 - "Color Contrast & Accessibility"
Cohesion: 0.13
Nodes (14): Asset Accessibility Checks (WCAG AA contrast, alt text), Color Palette Management, Color System Hierarchy (Primary, Secondary, Neutral, Semantic), luminance() / contrastRatio() Formula, WCAG 2.1 Contrast Ratios, Card Spec, Input Spec, Card Tokens (--card-*) (+6 more)

### Community 28 - "Social Photos Design"
Cohesion: 0.20
Nodes (15): Banner Typography Rules, Color Accessibility Considerations (WCAG AA 4.5:1), Social Photos Design Guide, assets-organizing skill, HTML Design Rules (exact viewport, self-contained, safe zones), Social Photo HTML Template Structure, Social Platform Sizes, Platform-Specific Tips (+7 more)

### Community 29 - "CIP Style Guide"
Cohesion: 0.19
Nodes (17): CIP Design Styles (summary table), CIP Style Modifiers, CIP Design Style Guide, Bold Dynamic style, Classic Traditional style, CIP Color Psychology, Corporate Minimal style, Fresh Modern style (+9 more)

### Community 32 - "Banner Export & Asset Approval"
Cohesion: 0.13
Nodes (14): assets-organizing Skill, chrome-devtools Skill, Banner Export to PNG via Screenshot, Banner Output Path Convention (assets/banners/{campaign}/), Asset Approval Checklist, Legal & Compliance Review, Reviewer Sign-off and Final Approval, Asset Organization Guide (+6 more)

### Community 33 - "CIP Deliverables"
Cohesion: 0.21
Nodes (15): CIP Deliverable Guide, Apparel (polo shirt, uniforms), Digital Assets (social media, email signature), Events & Promotional, Office Environment (signage, wayfinding, wall graphics), Stationery Set (business card, letterhead, envelope), Vehicle Branding, CIP Design Reference (+7 more)

### Community 34 - "Cross-File Data Contract Tests"
Cohesion: 0.23
Nodes (5): read_rows(), split_values(), style_identities(), TestGeneratedCatalogContract, TestStyleIdentityContract

### Community 35 - "CIP Image Generator"
Cohesion: 0.19
Nodes (7): build_cip_prompt(), check_logo_required(), generate_cip_set(), generate_with_nano_banana(), load_env(), load_logo_image(), main()

### Community 36 - "Design Token Starter: Spacing"
Cohesion: 0.12
Nodes (16): $type, $value, $type, $value, $type, $value, $type, $value (+8 more)

### Community 40 - "Brand Color Extractor"
Cohesion: 0.20
Nodes (11): calculateCompliance(), colorDistance(), displayPalette(), extractHexColors(), findNearestBrandColor(), fs, generateImageMagickCommand(), hexToRgb() (+3 more)

### Community 41 - "Token Sync & Validation Tests"
Cohesion: 0.23
Nodes (4): test_sync_parses_bundled_starter_template(), _run(), test_flags_hardcoded_hex_sharing_line_with_token(), test_token_only_line_reports_no_violation()

### Community 43 - "Palette Luminance Checks"
Cohesion: 0.18
Nodes (4): _palette_is_dark(), _relative_luminance(), TestEndToEndCoherence, TestLuminance

### Community 45 - "Banner Sizes & Art Direction"
Cohesion: 0.16
Nodes (8): Banner Sizes & Art Direction Styles Reference, 22 Art Direction Styles, Pinterest Research Queries, Platform Banner Sizes (Social, Display Ads, Website, Print), Print Specs (300 DPI, CMYK, bleed), Banner Design Skill, frontend-design Skill, ui-ux-pro-max Skill

### Community 46 - "Brand Asset Validator"
Cohesion: 0.25
Nodes (13): checkManifest(), formatBytes(), formatOutput(), fs, main(), parseFilename(), path, RULES (+5 more)

### Community 48 - "Reasoning Rule Contract"
Cohesion: 0.20
Nodes (5): apply_decision_rules(), _object_without_duplicates(), parse_decision_rules(), _validate_action(), TestReasoningContract

### Community 49 - "CIP HTML Renderer"
Cohesion: 0.22
Nodes (4): generate_html(), get_deliverable_info(), get_image_base64(), main()

### Community 50 - "Logo Generator"
Cohesion: 0.21
Nodes (5): enhance_prompt(), generate_batch(), generate_logo(), load_env(), main()

### Community 52 - "Dark Palette Derivation"
Cohesion: 0.22
Nodes (4): _contrast_ratio(), _derive_dark_palette(), _select_palette_for_mode(), TestPaletteSelection

### Community 55 - "Brand Voice & Messaging"
Cohesion: 0.18
Nodes (9): Brand Consistency Checklist, Brand Audit Frequency, Channel Audit (Website, Social, Email, Collateral), Messaging Framework, Core Statements (Mission, Vision, Value Proposition, Positioning), Message Architecture (Primary, Supporting, Elevator Pitches, Proof Points), Brand Voice Framework, Voice Chart and Context Adaptation (+1 more)

### Community 56 - "Text Layout Resilience Tests"
Cohesion: 0.18
Nodes (3): read_rows(), TestTextLayoutDataContracts, TestTextLayoutRetrieval

### Community 57 - "Token Embedder"
Cohesion: 0.17
Nodes (8): args, fs, minimal, MINIMAL_TOKENS, path, projectRoot, tokensPath, wrapStyle

### Community 58 - "Token Validator Script"
Cohesion: 0.24
Nodes (11): extensions, formatReport(), fs, getFiles(), main(), parseArgs(), path, patterns (+3 more)

### Community 59 - "Landing & Stack Contracts"
Cohesion: 0.20
Nodes (7): TestLandingAndStackContract, _check_file(), _check_provenance(), _read_rows(), _valid_catalog_source_key(), _valid_dataset_source_key(), _valid_provenance_source()

### Community 60 - "Piano package.json (Vite)"
Cohesion: 0.17
Nodes (11): devDependencies, vite, name, private, scripts, build, dev, preview (+3 more)

### Community 61 - "Component Specifications"
Cohesion: 0.20
Nodes (10): Design Components (Buttons, Spacing Scale, Border Radius), Component Specifications, Alert Spec, Badge Spec, Button Spec (variants, sizes, states, anatomy), Button Tokens (--button-*), Spacing Scale (4px base unit), States and Variants (+2 more)

### Community 62 - "UX Rule Categories & Forms"
Cohesion: 0.18
Nodes (10): Form with React Hook Form + Zod, Form with Validation Pattern (react-hook-form + zod), Charts & Data Rules (LOW: legends, tooltips, accessible colors), Forms & Feedback Rules (MEDIUM: visible labels, error placement, inline validation), Navigation Patterns Rules (HIGH: predictable back, bottom nav <=5, deep linking), Performance Rules (HIGH: image optimization, CLS, lazy loading), Searchable Local Design Data (styles, palettes, font pairings, UX guidelines, icons, GSAP presets, chart types, stacks), Style Selection Rules (HIGH: match product type, SVG icons not emoji) (+2 more)

### Community 63 - "Brand Context Injector"
Cohesion: 0.31
Nodes (10): extractColorsFromTable(), extractCoreAttributes(), extractHexColors(), extractImageStyle(), extractTypography(), extractVoice(), fs, generatePromptAddition() (+2 more)

### Community 64 - "Design Token Starter: Durations"
Cohesion: 0.18
Nodes (11): fast, normal, slow, $type, $value, $type, $value, primitive (+3 more)

### Community 65 - "Design Banner Sizes (duplicate)"
Cohesion: 0.29
Nodes (6): Banner Sizes & Art Direction Styles Reference, 22 Art Direction Styles, Complete Banner Sizes (social, display ads, website, print), Pinterest Research Queries, Print Specs (300 DPI, CMYK, bleed), Social Art Direction Styles (reused from Banner)

### Community 66 - "Slides HTML Template"
Cohesion: 0.29
Nodes (8): Slides Create Command Prompt, Chart.js Integration (v4.4.1 CDN, responsive chart container), Slide Design Token CSS Variables (--color-primary, --typography-font-heading, ...), embed-tokens.cjs (token embedding script output), HTML Slide Template, Slide Navigation Script (showSlide/nextSlide/prevSlide, keyboard, click, progress bar), slides create Subcommand, Slides Skill

### Community 68 - "Brand-to-Tokens Sync"
Cohesion: 0.29
Nodes (8): adjustBrightness(), { execFileSync }, extractColorsFromMarkdown(), fs, generateColorScale(), main(), path, updateDesignTokens()

### Community 69 - "Token CSS Generator"
Cohesion: 0.36
Nodes (9): flattenTokens(), fs, generateCSS(), generateTailwind(), main(), parseArgs(), path, resolveReference() (+1 more)

### Community 70 - "CIP Search CLI"
Cohesion: 0.28
Nodes (3): format_brief(), format_results(), main()

### Community 71 - "Reasoning & Stack Freshness Checks"
Cohesion: 0.25
Nodes (5): _check_reasoning_contract(), _check_stack_freshness_contract(), main(), _valid_date(), validate()

### Community 72 - "Catalog Contract Checks"
Cohesion: 0.28
Nodes (8): _catalog_date(), _check_catalog_contract(), _check_catalog_summary(), _check_font_catalog(), _check_phosphor_catalog(), _imported_weights(), _load_catalog_json(), _valid_google_fonts_exclusion_source()

### Community 73 - "Logo Search CLI"
Cohesion: 0.25
Nodes (3): Logo Workflow (brief, generate, ask preview, gallery), format_output(), generate_design_brief()

### Community 74 - "Slide Layout Patterns"
Cohesion: 0.25
Nodes (8): Slide Animation Classes (animate-fade-up, animate-count, animate-scale, animate-stagger), Background Image Slide with Gradient Overlay, Slide Component Variants (Card Styles, Metric Styles), Layout Patterns (25 slide layouts), Metrics Dashboard Layout (.slide-metrics), Two Column Split Layout (.slide-split), Visual Treatments (gradient-glow, bg-overlay, popular-highlight, ...), Animation Rules (MEDIUM: transform/opacity only, motion meaning, reduced motion, stagger)

### Community 76 - "Design Token Starter: Radius"
Cohesion: 0.29
Nodes (8): $type, $value, $type, $value, radius, default, full, default

### Community 77 - "Design System Persistence"
Cohesion: 0.29
Nodes (3): format_markdown(), generate_design_system(), TestPersistence

### Community 78 - "Graphify Project Instructions"
Cohesion: 0.48
Nodes (5): .claude/CLAUDE.md (graphify skill registration), graphify skill (/graphify trigger), CLAUDE.md (project instructions), GRAPH_REPORT.md (broad architecture review), Graphify Knowledge Graph (graphify-out/)

### Community 80 - "shadcn & Tailwind Theming"
Cohesion: 0.33
Nodes (7): shadcn CSS Variable System (--background, --primary, --radius in globals.css), Dark Mode Setup (next-themes ThemeProvider, ThemeToggle, localStorage fallback), shadcn/ui Theming & Customization, Semantic Colors and oklch Color Scales, Tailwind CSS Customization, @theme Directive (CSS-based custom tokens: colors, fonts, spacing, breakpoints), Typography & Color Rules (MEDIUM: semantic color tokens, type scale, dark mode)

### Community 82 - "Icon Design Reference"
Cohesion: 0.53
Nodes (5): Icon Design Reference, Icon Styles (15), Icon Generator CLI Options, Icon Categories (12), Icon Workflow (describe, style, generate, batch, multi-size)

### Community 83 - "Responsive Breakpoints"
Cohesion: 0.40
Nodes (5): Slide Responsive Breakpoints (768px tablet, 480px mobile, clamp typography), Feature Grid Layout (.slide-features), Tailwind Breakpoint System (sm 640, md 768, lg 1024, xl 1280, 2xl 1536), Tailwind CSS Responsive Design, Layout & Responsive Rules (HIGH: mobile-first, systematic breakpoints, no horizontal scroll)

### Community 84 - "Canvas Design System"
Cohesion: 0.33
Nodes (3): Canvas Design System, Design Movement Examples (Concrete Poetry, Chromatic Language, Analog Meditation, Organic Systems, Geometric Silence), Canvas Visual Design Layer

### Community 85 - "Design Token Starter: Shadow sm"
Cohesion: 0.47
Nodes (6): sm, shadow, sm, sm, $type, $value

### Community 88 - "Slide Decision System"
Cohesion: 0.40
Nodes (3): Slide Decision System CSVs, fetch-background.py (Pexels/Unsplash), search-slides.py (BM25 Slide Search)

### Community 89 - "Design Token Starter: lg"
Cohesion: 0.60
Nodes (5): lg, $type, $value, lg, lg

### Community 91 - "Design Token Starter: xl"
Cohesion: 0.67
Nodes (4): xl, xl, $type, $value

### Community 92 - "Design Token Starter: md"
Cohesion: 0.67
Nodes (4): $type, $value, md, md

### Community 93 - "Design Token Starter: none"
Cohesion: 0.67
Nodes (4): $type, $value, none, none

## Ambiguous Edges - Review These
- `Apache License 2.0 (ui-styling LICENSE.txt)` → `UI Styling Skill`  [AMBIGUOUS]
  .claude/skills/ui-styling/SKILL.md · relation: references

## Knowledge Gaps
- **61 isolated node(s):** `vite`, `ui-ux-pro-max Skill`, `frontend-design Skill`, `ai-artist Skill`, `ai-multimodal Skill` (+56 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 592 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **20 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Slide System` connect `Brand Skill & AI Generation` to `Three-Layer Token Architecture`, `Slide Decision System`, `Slide Token Validator (Legacy)`?**
  _High betweenness centrality (0.103) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `TailwindConfigGenerator` (e.g. with `TestGeneratedConfigIsValidJs` and `TestTailwindConfigGenerator`) actually correct?**
  _`TailwindConfigGenerator` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `vite`, `ui-ux-pro-max Skill`, `frontend-design Skill` to the rest of the system?**
  _61 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `shadcn Component Installer` be split into smaller, more focused modules?**
  _Cohesion score 0.0502283105022831 - nodes in this community are weakly interconnected._
- **Why does `Design Skill (SKILL.md)` connect `Design Skill Routing` to `CIP Deliverables`, `CIP & Logo Search Core`, `Design Slides Copy & Strategy`, `Logo Prompt & Style Guide`, `Social Photos Design`, `CIP Style Guide`?**
  _High betweenness centrality (0.100) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `DesignSystemGenerator` (e.g. with `TestReasoningMatch` and `TestReasoningContract`) actually correct?**
  _`DesignSystemGenerator` has 3 INFERRED edges - model-reasoned connections that need verification._

### Low-confidence Hints
_AMBIGUOUS edges — the extractor was unsure. Verify before acting on these._

- **What is the exact relationship between `Apache License 2.0 (ui-styling LICENSE.txt)` and `UI Styling Skill`?**
  _Edge tagged AMBIGUOUS (relation: references) - confidence is low._