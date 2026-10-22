# Hot key editor regression scope

Bounded targets: HotKeyService persistence and SettingsHotKeysViewModel draft behavior. Existing tests use xUnit v3, InMemoryConfiguration and R3. SDK-style net10.0; Avalonia 12.1.4; no new test dependencies.

Acceptance checklist: search by command/gesture, modified/unassigned/conflict filters, assignment and explicit conflict replacement, apply/cancel/reset, disabled persistence and default restoration, invalid batch rejection, page/service disposal. UI compiled bindings are validated by the build; actual interactive capture needs a host.
