export default {
  logo: (
    <>
      <img src="/logo/icon.png" alt="logo" width={32} style={{ marginRight: "0.5em" }} />
      <span>BLIT</span>
    </>
  ),
  project: {
    link: "https://github.com/0x1c-works/BLIT",
  },
  i18n: [
    { locale: "en", text: "English" },
    { locale: "zh", text: "中文" },
  ],
  head: (
    <>
      <link rel="icon" type="image/x-icon" href="/favicon.ico" />
    </>
  ),
  footer: {
    text: (
      <span class="nx-text-xs">
        {" "}
        <code>AGPLv3</code> {new Date().getFullYear()} © 0x1C Works with ❤️
      </span>
    ),
  },
  search: {
    placeholder: "Search...",
  },
  useNextSeoProps() {
    return { titleTemplate: "%s - BLIT the Bannerlord Image Tool" };
  },
};
