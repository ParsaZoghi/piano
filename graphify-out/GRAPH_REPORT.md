# Graph Report - piano  (2026-10-13)

## Corpus Check
- 154 files · ~231,276 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 55 file(s) not represented in the graph (top: .csv 53, (none) 1, .css 1)

## Summary
- 1879 nodes · 3480 edges · 122 communities (96 shown, 26 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 281 edges (avg confidence: 0.86)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `8685f152`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- TestShadcnInstaller
- scripts/core.py
- gray
- color
- cip/core.py
- card
- slide_search_core.py
- Slides Copywriting Formulas
- design_system.py
- spacing
- DesignSystemGenerator
- html-token-validator.py
- Design Skill (SKILL.md)
- test_core_data_quality.py
- generate-slide.py
- TestTailwindConfigGenerator
- Design System Skill
- validate_data.py
- test_design_system_mode.py
- Logo AI Prompt Engineering
- re
- fetch-background.py
- Brand Skill
- TestThresholdGate
- Brand Update Workflow (Gather, Edit Guidelines, Sync, Verify, Report)
- Copywriting Formulas (25 formulas for slide copy)
- UI Styling Skill
- Color Semantics (background, primary, secondary, muted, accent, destructive, status)
- Social Photos Design Guide
- CIP Design Style Guide
- PianoScene
- TestWebStackFreshness
- Asset Approval Checklist
- CIP Design Reference
- i18n.js
- cip/generate.py
- fontSize
- TestNativeDesktopStackFreshness
- extract-colors.cjs
- pathlib
- test_tailwind_config_gen.py
- _palette_is_dark
- CatalogRefreshTest
- Banner Design Workflow (Gather, Research, Generate, Export, Iterate)
- validate-asset.cjs
- TailwindConfigGenerator
- test_data_contracts.py
- render-html.py
- logo/generate.py
- main
- _select_palette_for_mode
- TestDomainDetection
- TestSearchDomains
- Brand Consistency Checklist
- test_text_layout_resilience.py
- embed-tokens.cjs
- validate-tokens.cjs
- t
- package.json
- Button Spec (variants, sizes, states, anatomy)
- Rule Categories by Priority (1-10)
- inject-brand-context.cjs
- primitive
- synth.js
- HTML Slide Template
- sync-brand-to-tokens.cjs
- generate-tokens.cjs
- argparse
- ShadcnInstaller
- lessonPlay.js
- logo/search.py
- Layout Patterns (25 slide layouts)
- subprocess
- radius
- generate_design_system
- CLAUDE.md (project instructions)
- notes.js
- shadcn CSS Variable System (--background, --primary, --radius in globals.css)
- Icon Design Reference
- Tailwind Breakpoint System (sm 640, md 768, lg 1024, xl 1280, 2xl 1536)
- Canvas Design System
- shadow
- test_core.py
- Contextual Decision Flow
- lg
- xl
- md
- none
- phone.js
- pitchName
- app.js
- Logo Design (built-in)
- TimedEngine
- devices.js
- Banner Sizes & Art Direction Styles Reference
- design-tokens-starter.json
- button
- FindEngine
- TestStyleTaxonomy
- input
- csv
- _check_font_catalog
- Keystage — research references
- $type
- radius
- padding-y
- destructive
- destructive-foreground
- muted
- primary-foreground
- ring
- secondary-foreground
- TestMetricMath
- TestFixtureValidation

## God Nodes (most connected - your core abstractions)
1. `TailwindConfigGenerator` - 58 edges
2. `t()` - 37 edges
3. `TestTailwindConfigGenerator` - 35 edges
4. `DesignSystemGenerator` - 35 edges
5. `ShadcnInstaller` - 34 edges
6. `Design Skill (SKILL.md)` - 34 edges
7. `mountLesson()` - 32 edges
8. `TestShadcnInstaller` - 26 edges
9. `PianoScene` - 21 edges
10. `icon()` - 21 edges

## Surprising Connections (you probably didn't know these)
- `generate-tokens.cjs Script` --semantically_similar_to--> `sync-brand-to-tokens.cjs Script`  [INFERRED] [semantically similar]
  .claude/skills/design-system/SKILL.md → .claude/skills/brand/SKILL.md
- `.claude/CLAUDE.md (graphify skill registration)` --conceptually_related_to--> `CLAUDE.md (project instructions)`  [INFERRED]
  .claude/CLAUDE.md → CLAUDE.md
- `When to Use Multiple Skills` --conceptually_related_to--> `Complete Brand Package Workflow (Logo, CIP, Presentation)`  [INFERRED]
  .claude/skills/design/references/design-routing.md → .claude/skills/design/SKILL.md
- `Social Photos Workflow (orchestrate to organize)` --semantically_similar_to--> `Social Photos 8-Step Workflow`  [INFERRED] [semantically similar]
  .claude/skills/design/SKILL.md → .claude/skills/design/references/social-photos-design.md
- `UI Styling Skill` --references--> `Apache License 2.0 (ui-styling LICENSE.txt)`  [AMBIGUOUS]
  .claude/skills/ui-styling/SKILL.md → .claude/skills/ui-styling/LICENSE.txt

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Complete Brand Package Flow (Logo to CIP to Presentation)** — _claude_skills_design_skill_logo_design, _claude_skills_design_skill_cip_design, _claude_skills_design_skill_slides [EXTRACTED 1.00]
- **Design System Skill Chain (brand, design-system, ui-styling)** — _claude_skills_design_skill_brand_skill, _claude_skills_design_skill_design_system_skill, _claude_skills_design_skill_ui_styling_skill [EXTRACTED 1.00]
- **Slides Knowledge Base (create, layouts, template, copywriting, strategies)** — _claude_skills_design_references_slides_create, _claude_skills_design_references_slides_layout_patterns, _claude_skills_design_references_slides_html_template, _claude_skills_design_references_slides_copywriting_formulas, _claude_skills_design_references_slides_strategies [EXTRACTED 1.00]
- **Banner Production Skill Chain** — _claude_skills_banner_design_skill_ui_ux_pro_max_skill, _claude_skills_banner_design_skill_frontend_design_skill, _claude_skills_banner_design_skill_ai_artist_skill, _claude_skills_banner_design_skill_ai_multimodal_skill, _claude_skills_banner_design_skill_chrome_devtools_skill, _claude_skills_brand_skill_inject_brand_context_cjs [EXTRACTED 1.00]
- **Brand Guidelines to Design Tokens Sync Pipeline** — _claude_skills_brand_skill_brand_guidelines_md, _claude_skills_brand_skill_sync_brand_to_tokens_cjs, _claude_skills_brand_skill_design_tokens_json, _claude_skills_brand_skill_design_tokens_css, _claude_skills_brand_skill_inject_brand_context_cjs [EXTRACTED 1.00]
- **Slides Skill Knowledge Base (strategy + layout + copy + template)** — _claude_skills_slides_references_slide_strategies_slide_strategies, _claude_skills_slides_references_layout_patterns_layout_patterns, _claude_skills_slides_references_copywriting_formulas_copywriting_formulas, _claude_skills_slides_references_html_template_html_slide_template, _claude_skills_slides_references_copywriting_formulas_search_slides_py [EXTRACTED 1.00]
- **Three-Layer Token System (Primitive, Semantic, Component)** — _claude_skills_design_system_references_primitive_tokens, _claude_skills_design_system_references_semantic_tokens, _claude_skills_design_system_references_component_tokens, _claude_skills_design_system_references_token_architecture [EXTRACTED 1.00]
- **UI Styling Core Stack (component layer + styling layer + visual design layer)** — _claude_skills_ui_styling_skill_shadcn_ui, _claude_skills_ui_styling_skill_tailwind_css, _claude_skills_ui_styling_skill_canvas_visual_design_layer, _claude_skills_ui_styling_skill_radix_ui [EXTRACTED 1.00]
- **Emotion-Driven Slide Design (emotion arc -> formula mapping -> layout decision)** — _claude_skills_slides_references_slide_strategies_emotion_arc, _claude_skills_slides_references_copywriting_formulas_formula_to_slide_mapping, _claude_skills_slides_references_layout_patterns_layout_decision_flow, _claude_skills_slides_references_slide_strategies_duarte_sparkline [INFERRED 0.85]

## Communities (122 total, 26 thin omitted)

### Community 1 - "scripts/core.py"
Cohesion: 0.06
Nodes (31): BM25, _contains_phrase(), detect_domain(), _domain_keywords(), _exact_match_diagnostic(), _exact_row_identity(), _exact_stack_identifier(), _file_signature() (+23 more)

### Community 2 - "gray"
Cohesion: 0.05
Nodes (53): $type, $value, $type, $value, $type, $value, $type, $value (+45 more)

### Community 3 - "color"
Cohesion: 0.11
Nodes (19): $type, $value, background, foreground, muted-foreground, primary, primary-hover, secondary (+11 more)

### Community 4 - "cip/core.py"
Cohesion: 0.06
Nodes (13): BM25, detect_domain(), get_cip_brief(), _load_csv(), search(), search_all(), _search_csv(), BM25 (+5 more)

### Community 5 - "card"
Cohesion: 0.20
Nodes (12): $type, $value, bg, bg, padding, shadow, card, bg (+4 more)

### Community 6 - "slide_search_core.py"
Cohesion: 0.08
Nodes (17): format_context(), format_result(), main(), BM25, calculate_pattern_break(), detect_domain(), get_background_config(), get_color_for_emotion() (+9 more)

### Community 7 - "Slides Copywriting Formulas"
Cohesion: 0.12
Nodes (33): Slides Reference, Slides Copywriting Formulas, AIDA (Attention-Interest-Desire-Action), Before-After-Bridge, Cost of Inaction formula, FAB (Features-Advantages-Benefits), Formula-to-Slide Mapping, Headline Patterns (+25 more)

### Community 8 - "design_system.py"
Cohesion: 0.09
Nodes (12): ansi_ljust(), _detect_page_type(), format_ascii_box(), format_master_md(), format_page_override_md(), _generate_intelligent_overrides(), hex_to_ansi(), persist_design_system() (+4 more)

### Community 9 - "spacing"
Cohesion: 0.06
Nodes (34): $type, $value, $type, $value, $type, $value, $type, $value (+26 more)

### Community 10 - "DesignSystemGenerator"
Cohesion: 0.11
Nodes (4): DesignSystemGenerator, TestReasoningMatch, read_rows(), TestReasoningContract

### Community 11 - "html-token-validator.py"
Cohesion: 0.12
Nodes (12): get_context(), is_allowed_exception(), is_allowed_rgba(), is_inside_block(), load_css_variables(), main(), print_result(), print_summary() (+4 more)

### Community 12 - "Design Skill (SKILL.md)"
Cohesion: 0.21
Nodes (22): Design Routing Guide, Multi-Skill Workflows (setup, migration, component creation), Routing by Question Type, Routing by Task Type, When to Use Multiple Skills, Design Skill (SKILL.md), ai-artist skill, ai-multimodal skill (+14 more)

### Community 13 - "test_core_data_quality.py"
Cohesion: 0.13
Nodes (12): read_rows(), TestAccessibilityGuidance, TestChartsTypographyAndIcons, TestCurrentReactGuidance, TestSemanticColors, _check_chart_contract(), _check_typography_contract(), _configured_font_names() (+4 more)

### Community 14 - "generate-slide.py"
Cohesion: 0.13
Nodes (11): _e(), generate_chart_slide(), generate_cta_slide(), generate_deck(), generate_metrics_slide(), generate_problem_slide(), generate_solution_slide(), generate_testimonial_slide() (+3 more)

### Community 16 - "Design System Skill"
Cohesion: 0.12
Nodes (17): Dialog Spec, Table Spec, Component Tokens, Dialog/Modal Tokens (--dialog-*), Table Tokens (--table-*), Primitive Tokens, Semantic Tokens, Focus Ring Spec (+9 more)

### Community 17 - "validate_data.py"
Cohesion: 0.12
Nodes (24): _check_app_interface_contract(), _check_color_contract(), _check_core_data_contract(), _check_file(), _check_icon_contract(), _check_landing_claims(), _check_motion_contract(), _check_provenance() (+16 more)

### Community 18 - "test_design_system_mode.py"
Cohesion: 0.14
Nodes (6): _filter_anti_patterns_for_mode(), _query_wants_dark(), _resolve_color_mode(), _style_is_dark_primary(), TestAntiPatternGating, TestModeResolution

### Community 19 - "Logo AI Prompt Engineering"
Cohesion: 0.17
Nodes (14): Core Identity (primary logo, variations), CIP Negative Prompts, Logo Available Styles (55+), Logo AI Prompt Engineering, Logo Core Prompt Structure, Effective Keywords by Style, Logo Negative Prompts, Logo Prompt Templates (+6 more)

### Community 20 - "re"
Cohesion: 0.19
Nodes (8): apply_color(), apply_viewbox_size(), extract_svgs(), generate_batch(), generate_icon(), generate_sizes(), load_env(), main()

### Community 21 - "fetch-background.py"
Cohesion: 0.16
Nodes (9): generate_css_for_background(), get_background_image(), get_curated_images(), get_overlay_css(), get_pexels_search_url(), load_backgrounds_config(), load_brand_colors(), main() (+1 more)

### Community 22 - "Brand Skill"
Cohesion: 0.12
Nodes (18): Post-Approval Archival, Asset Directory Structure (.assets metadata + assets raw files), manifest.json Asset Registry, Asset Tagging System, Logo Usage Rules, Incorrect Logo Usage (Don'ts), Logo Variants (Full Horizontal, Stacked, Icon, Wordmark, Color Variants), Typography Specifications (+10 more)

### Community 24 - "Brand Update Workflow (Gather, Edit Guidelines, Sync, Verify, Report)"
Cohesion: 0.18
Nodes (16): ai-artist Skill, ai-multimodal Skill, AI Visual Generation (Gemini Flash vs Pro image models), Brand Guidelines Template, Extractable Brand Fields (colors, typography, voice, logo), Brand Color Presets (ocean-professional, electric-creative, forest-calm, midnight-purple, sunset-warm), docs/brand-guidelines.md (Brand Source of Truth), assets/design-tokens.css (+8 more)

### Community 25 - "Copywriting Formulas (25 formulas for slide copy)"
Cohesion: 0.17
Nodes (14): AIDA (Attention-Interest-Desire-Action), Before-After-Bridge (BAB), Copywriting Formulas (25 formulas for slide copy), Cost of Inaction Formula, FAB (Features-Advantages-Benefits), Headline Patterns (Power Words, Contrast, Social Proof), PAS (Problem-Agitate-Solution), search-slides.py (design-system slide knowledge search script) (+6 more)

### Community 26 - "UI Styling Skill"
Cohesion: 0.16
Nodes (17): Apache License 2.0 (ui-styling LICENSE.txt), Keyboard Navigation and Focus Management (focus-visible, skip link, dialog focus trap), Screen Reader Support (semantic HTML, ARIA labels), shadcn/ui Accessibility Patterns, Form & Input Components (Button, Input, Form, Select, Checkbox, Radio Group), shadcn/ui Component Reference, Layout Utilities (Display, Flexbox, Grid, Positioning, Z-Index), Tailwind CSS Utility Reference (+9 more)

### Community 27 - "Color Semantics (background, primary, secondary, muted, accent, destructive, status)"
Cohesion: 0.13
Nodes (14): Asset Accessibility Checks (WCAG AA contrast, alt text), Color Palette Management, Color System Hierarchy (Primary, Secondary, Neutral, Semantic), luminance() / contrastRatio() Formula, WCAG 2.1 Contrast Ratios, Card Spec, Input Spec, Card Tokens (--card-*) (+6 more)

### Community 28 - "Social Photos Design Guide"
Cohesion: 0.22
Nodes (14): Banner Typography Rules, Color Accessibility Considerations (WCAG AA 4.5:1), Social Photos Design Guide, assets-organizing skill, HTML Design Rules (exact viewport, self-contained, safe zones), Social Photo HTML Template Structure, Social Platform Sizes, Platform-Specific Tips (+6 more)

### Community 29 - "CIP Design Style Guide"
Cohesion: 0.31
Nodes (11): CIP Design Styles (summary table), CIP Style Modifiers, CIP Design Style Guide, Bold Dynamic style, Classic Traditional style, Corporate Minimal style, Fresh Modern style, Luxury Premium style (+3 more)

### Community 30 - "PianoScene"
Cohesion: 0.05
Nodes (26): three, BLACK_NUDGE, blackGeometry, COLORS, Keyboard3D, WHITE_LENGTH, WHITE_TOP, whiteGeometry (+18 more)

### Community 32 - "Asset Approval Checklist"
Cohesion: 0.13
Nodes (14): assets-organizing Skill, chrome-devtools Skill, Banner Export to PNG via Screenshot, Banner Output Path Convention (assets/banners/{campaign}/), Asset Approval Checklist, Legal & Compliance Review, Reviewer Sign-off and Final Approval, Asset Organization Guide (+6 more)

### Community 33 - "CIP Design Reference"
Cohesion: 0.19
Nodes (16): CIP Deliverable Guide, Apparel (polo shirt, uniforms), Digital Assets (social media, email signature), Events & Promotional, Office Environment (signage, wayfinding, wall graphics), Stationery Set (business card, letterhead, envelope), Vehicle Branding, CIP Design Reference (+8 more)

### Community 34 - "i18n.js"
Cohesion: 0.12
Nodes (17): index.html (piano app entry page), #app mount element, DICTIONARIES, digits, has(), lookup(), RTL, setLanguage() (+9 more)

### Community 35 - "cip/generate.py"
Cohesion: 0.18
Nodes (7): build_cip_prompt(), check_logo_required(), generate_cip_set(), generate_with_nano_banana(), load_env(), load_logo_image(), main()

### Community 36 - "fontSize"
Cohesion: 0.12
Nodes (16): $type, $value, $type, $value, $type, $value, $type, $value (+8 more)

### Community 40 - "extract-colors.cjs"
Cohesion: 0.20
Nodes (11): calculateCompliance(), colorDistance(), displayPalette(), extractHexColors(), findNearestBrandColor(), fs, generateImageMagickCommand(), hexToRgb() (+3 more)

### Community 41 - "pathlib"
Cohesion: 0.18
Nodes (3): validate-tokens.cjs Script, main(), main()

### Community 43 - "_palette_is_dark"
Cohesion: 0.18
Nodes (4): _palette_is_dark(), _relative_luminance(), TestEndToEndCoherence, TestLuminance

### Community 45 - "Banner Design Workflow (Gather, Research, Generate, Export, Iterate)"
Cohesion: 0.16
Nodes (8): Banner Sizes & Art Direction Styles Reference, 22 Art Direction Styles, Pinterest Research Queries, Platform Banner Sizes (Social, Display Ads, Website, Print), Print Specs (300 DPI, CMYK, bleed), Banner Design Skill, frontend-design Skill, ui-ux-pro-max Skill

### Community 46 - "validate-asset.cjs"
Cohesion: 0.25
Nodes (13): checkManifest(), formatBytes(), formatOutput(), fs, main(), parseFilename(), path, RULES (+5 more)

### Community 48 - "test_data_contracts.py"
Cohesion: 0.10
Nodes (9): apply_decision_rules(), _object_without_duplicates(), parse_decision_rules(), _validate_action(), split_values(), style_identities(), TestLandingAndStackContract, TestStyleIdentityContract (+1 more)

### Community 49 - "render-html.py"
Cohesion: 0.25
Nodes (4): generate_html(), get_deliverable_info(), get_image_base64(), main()

### Community 50 - "logo/generate.py"
Cohesion: 0.21
Nodes (5): enhance_prompt(), generate_batch(), generate_logo(), load_env(), main()

### Community 52 - "_select_palette_for_mode"
Cohesion: 0.22
Nodes (4): _contrast_ratio(), _derive_dark_palette(), _select_palette_for_mode(), TestPaletteSelection

### Community 55 - "Brand Consistency Checklist"
Cohesion: 0.18
Nodes (9): Brand Consistency Checklist, Brand Audit Frequency, Channel Audit (Website, Social, Email, Collateral), Messaging Framework, Core Statements (Mission, Vision, Value Proposition, Positioning), Message Architecture (Primary, Supporting, Elevator Pitches, Proof Points), Brand Voice Framework, Voice Chart and Context Adaptation (+1 more)

### Community 56 - "test_text_layout_resilience.py"
Cohesion: 0.18
Nodes (3): read_rows(), TestTextLayoutDataContracts, TestTextLayoutRetrieval

### Community 57 - "embed-tokens.cjs"
Cohesion: 0.17
Nodes (8): args, fs, minimal, MINIMAL_TOKENS, path, projectRoot, tokensPath, wrapStyle

### Community 58 - "validate-tokens.cjs"
Cohesion: 0.24
Nodes (11): extensions, formatReport(), fs, getFiles(), main(), parseArgs(), path, patterns (+3 more)

### Community 59 - "t"
Cohesion: 0.27
Nodes (20): fill(), num(), t(), lessonGoal(), lessonTitle(), escapeHtml(), icon(), PATHS (+12 more)

### Community 60 - "package.json"
Cohesion: 0.08
Nodes (24): dependencies, qrcode, three, vazir-font, devDependencies, vite, ws, name (+16 more)

### Community 61 - "Button Spec (variants, sizes, states, anatomy)"
Cohesion: 0.20
Nodes (10): Design Components (Buttons, Spacing Scale, Border Radius), Component Specifications, Alert Spec, Badge Spec, Button Spec (variants, sizes, states, anatomy), Button Tokens (--button-*), Spacing Scale (4px base unit), States and Variants (+2 more)

### Community 62 - "Rule Categories by Priority (1-10)"
Cohesion: 0.18
Nodes (10): Form with React Hook Form + Zod, Form with Validation Pattern (react-hook-form + zod), Charts & Data Rules (LOW: legends, tooltips, accessible colors), Forms & Feedback Rules (MEDIUM: visible labels, error placement, inline validation), Navigation Patterns Rules (HIGH: predictable back, bottom nav <=5, deep linking), Performance Rules (HIGH: image optimization, CLS, lazy loading), Searchable Local Design Data (styles, palettes, font pairings, UX guidelines, icons, GSAP presets, chart types, stacks), Style Selection Rules (HIGH: match product type, SVG icons not emoji) (+2 more)

### Community 63 - "inject-brand-context.cjs"
Cohesion: 0.31
Nodes (10): extractColorsFromTable(), extractCoreAttributes(), extractHexColors(), extractImageStyle(), extractTypography(), extractVoice(), fs, generatePromptAddition() (+2 more)

### Community 64 - "primitive"
Cohesion: 0.18
Nodes (11): fast, normal, slow, $type, $value, $type, $value, primitive (+3 more)

### Community 65 - "synth.js"
Cohesion: 0.20
Nodes (9): hold(), release(), sustained, synth, voices, midiToFreq(), BEAT_LABELS, POINTS (+1 more)

### Community 66 - "HTML Slide Template"
Cohesion: 0.29
Nodes (8): Slides Create Command Prompt, Chart.js Integration (v4.4.1 CDN, responsive chart container), Slide Design Token CSS Variables (--color-primary, --typography-font-heading, ...), embed-tokens.cjs (token embedding script output), HTML Slide Template, Slide Navigation Script (showSlide/nextSlide/prevSlide, keyboard, click, progress bar), slides create Subcommand, Slides Skill

### Community 68 - "sync-brand-to-tokens.cjs"
Cohesion: 0.29
Nodes (8): adjustBrightness(), { execFileSync }, extractColorsFromMarkdown(), fs, generateColorScale(), main(), path, updateDesignTokens()

### Community 69 - "generate-tokens.cjs"
Cohesion: 0.36
Nodes (9): flattenTokens(), fs, generateCSS(), generateTailwind(), main(), parseArgs(), path, resolveReference() (+1 more)

### Community 70 - "argparse"
Cohesion: 0.31
Nodes (3): format_brief(), format_results(), main()

### Community 72 - "lessonPlay.js"
Cohesion: 0.15
Nodes (14): parseNote(), buildPrompts(), lessonIndex(), lessonRange(), LESSONS, parseSong(), UNITS, WHITE_5 (+6 more)

### Community 73 - "logo/search.py"
Cohesion: 0.25
Nodes (3): Logo Workflow (brief, generate, ask preview, gallery), format_output(), generate_design_brief()

### Community 74 - "Layout Patterns (25 slide layouts)"
Cohesion: 0.25
Nodes (8): Slide Animation Classes (animate-fade-up, animate-count, animate-scale, animate-stagger), Background Image Slide with Gradient Overlay, Slide Component Variants (Card Styles, Metric Styles), Layout Patterns (25 slide layouts), Metrics Dashboard Layout (.slide-metrics), Two Column Split Layout (.slide-split), Visual Treatments (gradient-glow, bg-overlay, popular-highlight, ...), Animation Rules (MEDIUM: transform/opacity only, motion meaning, reduced motion, stagger)

### Community 75 - "subprocess"
Cohesion: 0.23
Nodes (4): test_sync_parses_bundled_starter_template(), _run(), test_flags_hardcoded_hex_sharing_line_with_token(), test_token_only_line_reports_no_violation()

### Community 76 - "radius"
Cohesion: 0.29
Nodes (8): $type, $value, $type, $value, radius, default, full, default

### Community 77 - "generate_design_system"
Cohesion: 0.15
Nodes (4): format_markdown(), generate_design_system(), format_output(), TestPersistence

### Community 78 - "CLAUDE.md (project instructions)"
Cohesion: 0.48
Nodes (5): .claude/CLAUDE.md (graphify skill registration), graphify skill (/graphify trigger), CLAUDE.md (project instructions), GRAPH_REPORT.md (broad architecture review), Graphify Knowledge Graph (graphify-out/)

### Community 79 - "notes.js"
Cohesion: 0.11
Nodes (19): held, listeners, BLACK, LETTER_INDEX, NAMINGS, PARSE_NAMES, PIANO_MAX, PIANO_MIN (+11 more)

### Community 80 - "shadcn CSS Variable System (--background, --primary, --radius in globals.css)"
Cohesion: 0.33
Nodes (7): shadcn CSS Variable System (--background, --primary, --radius in globals.css), Dark Mode Setup (next-themes ThemeProvider, ThemeToggle, localStorage fallback), shadcn/ui Theming & Customization, Semantic Colors and oklch Color Scales, Tailwind CSS Customization, @theme Directive (CSS-based custom tokens: colors, fonts, spacing, breakpoints), Typography & Color Rules (MEDIUM: semantic color tokens, type scale, dark mode)

### Community 82 - "Icon Design Reference"
Cohesion: 0.53
Nodes (5): Icon Design Reference, Icon Styles (15), Icon Generator CLI Options, Icon Categories (12), Icon Workflow (describe, style, generate, batch, multi-size)

### Community 83 - "Tailwind Breakpoint System (sm 640, md 768, lg 1024, xl 1280, 2xl 1536)"
Cohesion: 0.40
Nodes (5): Slide Responsive Breakpoints (768px tablet, 480px mobile, clamp typography), Feature Grid Layout (.slide-features), Tailwind Breakpoint System (sm 640, md 768, lg 1024, xl 1280, 2xl 1536), Tailwind CSS Responsive Design, Layout & Responsive Rules (HIGH: mobile-first, systematic breakpoints, no horizontal scroll)

### Community 84 - "Canvas Design System"
Cohesion: 0.33
Nodes (3): Canvas Design System, Design Movement Examples (Concrete Poetry, Chromatic Language, Analog Meditation, Organic Systems, Geometric Silence), Canvas Visual Design Layer

### Community 85 - "shadow"
Cohesion: 0.47
Nodes (6): sm, shadow, sm, sm, $type, $value

### Community 87 - "test_core.py"
Cohesion: 0.11
Nodes (3): TestBm25CoreBehavior, TestDiagnosticsContracts, TestTokenizer

### Community 88 - "Contextual Decision Flow"
Cohesion: 0.40
Nodes (3): Slide Decision System CSVs, fetch-background.py (Pexels/Unsplash), search-slides.py (BM25 Slide Search)

### Community 89 - "lg"
Cohesion: 0.60
Nodes (5): lg, $type, $value, lg, lg

### Community 91 - "xl"
Cohesion: 0.67
Nodes (4): xl, xl, $type, $value

### Community 92 - "md"
Cohesion: 0.67
Nodes (4): $type, $value, md, md

### Community 93 - "none"
Cohesion: 0.67
Nodes (4): $type, $value, none, none

### Community 94 - "phone.js"
Cohesion: 0.31
Nodes (9): BLACK, LETTERS, mountPhone(), connect(), press(), release(), releaseAll(), send() (+1 more)

### Community 95 - "pitchName"
Cohesion: 0.38
Nodes (9): isBlack(), noteName(), octaveOf(), pitchName(), staffStep(), showFindPrompt(), showTimedPrompt(), staffSvg() (+1 more)

### Community 96 - "app.js"
Cohesion: 0.21
Nodes (16): CAMERA_KEYS, startApp(), route(), LANGUAGES, defaults, listeners, onSettingsChange(), saveSettings() (+8 more)

### Community 97 - "Logo Design (built-in)"
Cohesion: 0.22
Nodes (13): CIP Color Psychology, Logo Color Psychology, Color Harmony Types, Primary Color Meanings, Quick Reference Palettes, Logo Design Reference, Logo Color Psychology (summary), Logo Industry Defaults (+5 more)

### Community 99 - "devices.js"
Cohesion: 0.15
Nodes (13): input, listeners, midi, connect(), listeners, notify(), remoteHost, build() (+5 more)

### Community 100 - "Banner Sizes & Art Direction Styles Reference"
Cohesion: 0.29
Nodes (6): Banner Sizes & Art Direction Styles Reference, 22 Art Direction Styles, Complete Banner Sizes (social, display ads, website, print), Pinterest Research Queries, Print Specs (300 DPI, CMYK, bleed), Social Art Direction Styles (reused from Banner)

### Community 101 - "design-tokens-starter.json"
Cohesion: 0.15
Nodes (12): component, $type, $value, dark, semantic, $schema, $type, $value (+4 more)

### Community 102 - "button"
Cohesion: 0.20
Nodes (10): fg, font-size, hover-bg, button, $type, $value, $type, $value (+2 more)

### Community 106 - "input"
Cohesion: 0.29
Nodes (8): padding-x, input, $type, $value, focus-ring, padding-x, $type, $value

### Community 109 - "_check_font_catalog"
Cohesion: 0.29
Nodes (7): TestGeneratedCatalogContract, _catalog_date(), _check_catalog_contract(), _check_catalog_summary(), _check_font_catalog(), _check_phosphor_catalog(), _valid_google_fonts_exclusion_source()

### Community 110 - "Keystage — research references"
Cohesion: 0.33
Nodes (5): A. Game-based learning and gamification in music / piano, B. Technology-supported piano tutoring and feedback, C. Rhythm, note reading and beginner pedagogy, How the papers shaped the game, Keystage — research references

### Community 111 - "$type"
Cohesion: 0.60
Nodes (5): $type, $value, border, border, border

### Community 112 - "radius"
Cohesion: 0.60
Nodes (5): radius, radius, radius, $type, $value

### Community 113 - "padding-y"
Cohesion: 0.67
Nodes (4): padding-y, padding-y, $type, $value

### Community 114 - "destructive"
Cohesion: 0.67
Nodes (3): destructive, $type, $value

### Community 115 - "destructive-foreground"
Cohesion: 0.67
Nodes (3): destructive-foreground, $type, $value

### Community 116 - "muted"
Cohesion: 0.67
Nodes (3): muted, $type, $value

### Community 117 - "primary-foreground"
Cohesion: 0.67
Nodes (3): primary-foreground, $type, $value

### Community 118 - "ring"
Cohesion: 0.67
Nodes (3): ring, $type, $value

### Community 119 - "secondary-foreground"
Cohesion: 0.67
Nodes (3): secondary-foreground, $type, $value

## Ambiguous Edges - Review These
- `UI Styling Skill` → `Apache License 2.0 (ui-styling LICENSE.txt)`  [AMBIGUOUS]
  .claude/skills/ui-styling/SKILL.md · relation: references

## Knowledge Gaps
- **61 isolated node(s):** `vazir-font`, `Brand Subcommand Routing`, `ai-artist Skill`, `ai-multimodal Skill`, `Chart.js Integration` (+56 more)
  These have ≤1 connection - possible missing edges or undocumented components. (Counts symbols only; 689 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **26 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Design Skill (SKILL.md)` connect `Design Skill (SKILL.md)` to `CIP Design Reference`, `Logo Design (built-in)`, `cip/core.py`, `Slides Copywriting Formulas`, `Logo AI Prompt Engineering`, `Social Photos Design Guide`, `CIP Design Style Guide`?**
  _High betweenness centrality (0.058) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `TailwindConfigGenerator` (e.g. with `TestGeneratedConfigIsValidJs` and `TestTailwindConfigGenerator`) actually correct?**
  _`TailwindConfigGenerator` has 2 INFERRED edges - model-reasoned connections that need verification._
- **What connects `vazir-font`, `Brand Subcommand Routing`, `ai-artist Skill` to the rest of the system?**
  _61 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `TestShadcnInstaller` be split into smaller, more focused modules?**
  _Cohesion score 0.08735632183908046 - nodes in this community are weakly interconnected._
- **Why does `TailwindConfigGenerator` connect `TailwindConfigGenerator` to `._base_config`, `.generate_config_string`, `.add_colors`, `test_tailwind_config_gen.py`, `TestTailwindConfigGenerator`, `.add_plugins`, `main`, `.recommend_plugins`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Are the 3 inferred relationships involving `DesignSystemGenerator` (e.g. with `TestReasoningMatch` and `TestReasoningContract`) actually correct?**
  _`DesignSystemGenerator` has 3 INFERRED edges - model-reasoned connections that need verification._

### Low-confidence Hints
_AMBIGUOUS edges — the extractor was unsure. Verify before acting on these._

- **What is the exact relationship between `UI Styling Skill` and `Apache License 2.0 (ui-styling LICENSE.txt)`?**
  _Edge tagged AMBIGUOUS (relation: references) - confidence is low._