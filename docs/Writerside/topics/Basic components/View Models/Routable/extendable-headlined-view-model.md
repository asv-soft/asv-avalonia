# Extendable Headlined View Model

[`ExtendableHeadlinedViewModel<TSelfInterface>`](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/ViewModel/Extendable/ExtendableHeadlinedViewModel.cs)
is an abstract view model that derives from [`ViewModel<TExtensionIfc>`](extendable-view-model.md) — passing
`TSelfInterface` as the extension interface — and implements `IHeadlinedViewModel`, adding a header, icon, description,
visibility and order.

It is a sibling of [`HeadlinedViewModel`](headlined-view-model.md) rather than a subclass: both implement
`IHeadlinedViewModel`, but `HeadlinedViewModel` builds on the plain [`ViewModel`](view-model.md) base, while this class
builds on the extensible one. The type parameter is constrained to `where TSelfInterface : class, ISupportId<NavId>`.
