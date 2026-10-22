# Hot key editor validation plan

1. HotKeyServiceTest: round-trip disabled/custom/default assignments; validate entire batch before mutation; change events observe completed batch.
2. SettingsHotKeysViewModelTest: search and filters, conflict refusal/replacement, changes remain drafts until Apply, Cancel reloads, Reset all changes drafts, disposal.
3. Run filtered HotKeys tests, build solution once, inspect all assertions against requirements.

No source-pairing/test-quality agent tools are available. Use the bounded inventory above and inline assertion review. Preserve all unrelated IO work.
