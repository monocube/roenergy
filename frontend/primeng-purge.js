const glob = require('glob');
const fs = require('fs');
const path = require('path');

function findMatches(regex, str, matches = []) {
  const res = regex.exec(str);
  res && matches.push(res) && findMatches(regex, str, matches);
  return matches;
}

function getImportedPrimeNgModules() {
  // Make sure it's a list with unique items
  const primeNgImports = new Set();
  // The components are always declared in a module, so we just have to look through those
  glob.sync('src/**/*.ts', { nodir: true }).forEach((file) => {
    const fileContents = fs.readFileSync(file);

    const matches = findMatches(/import .*primeng\/(?!api)([a-zA-Z]*)/g, fileContents);
    for (const match of matches) {
      primeNgImports.add(match[1]);
    }
  });

  return Array.from(primeNgImports);
}

module.exports = {
  /**
   * We use PrimeNg in this project, but we also wish to keep the css-filesize as small as possible.
   * In order to keep the css size small, we use a purger, see webpack.config.js. However, this purger won't 'know' what css styles PrimeNg is using,
   * because those templates are defined in the primeng node_modules.
   */
  getPrimeNgTemplateFiles: function () {
    const primeNgModules = getImportedPrimeNgModules();

    const componentTemplatePaths = [];
    primeNgModules.forEach((pModule) => {
      // Use glob to make this as robust as possible. We are looking for something like /primeng/button/button.mjs
      const templatePaths = glob.sync(
        path.join('node_modules', 'primeng', '**', pModule, `${pModule}\.*js`)
      );
      if (!templatePaths.length) {
        throw new Error(`
            Something went wrong while finding the PrimeNg node_modules for '${pModule}'. 
            This probably means that something has changed in the component library, and you'll have to find the files where the templates are defined.
        `);
      }
      if (templatePaths.length > 1) {
        console.warn(
          `
            We found multiple PrimeNg modules for '${pModule}', is that right? 
            If it is, you can probably remove this warning, but usually there should only be one template file.
            If it's not, you should probably change the glob ↑ , because that means unnecessary css is left in the styles.css. We're now including: 
        `,
          templatePaths
        );
      }

      // This should only return one templatePath, but to make it more robust, let's assume it could also have found more than one.
      templatePaths.forEach((templatePath) =>
        componentTemplatePaths.push(templatePath)
      );
    });

    if (!componentTemplatePaths.length) {
      throw new Error(`
            We couldn't find any templates for PrimeNg components. 
            That can't be right, unless you're not using PrimeNg anymore. 
            In that case, remove the reference to this function from webpack.config.js
        `);
    }
    return {
      paths: componentTemplatePaths,
      modules: primeNgModules,
    };
  },
};
