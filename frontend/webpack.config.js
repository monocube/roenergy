const glob = require("glob");
const { PurgeCSSPlugin } = require("purgecss-webpack-plugin");
const { getPrimeNgTemplateFiles } = require("./primeng-purge");

const primeNgTemplateFiles = getPrimeNgTemplateFiles();

module.exports = {
  plugins: [
    new PurgeCSSPlugin({
      safelist: () => ({
        deep: primeNgTemplateFiles.modules.map(
          (module) => new RegExp(`${module}`)
        ),
      }),
      defaultExtractor: (content) => content.match(/[\w-/:.]+(?<!:)/g) || [],
      paths: () =>
        glob
          .sync("./src/**/*", { nodir: true })
          .concat(primeNgTemplateFiles.paths),
    }),
  ],
};
