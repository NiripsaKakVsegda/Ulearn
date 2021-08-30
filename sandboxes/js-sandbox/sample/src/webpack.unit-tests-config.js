const path = require("path");
const glob = require("webpack-glob-entries");

module.exports = {
  mode: "development",
  entry: glob(path.resolve(__dirname, "*.test.?(m)js")),
  output: {
    path: path.resolve(__dirname, "dist"),
    filename: "[name].js",
  },
  node: false,
  module: {
    rules: [
      {
        test: /\.m?js$/,
        exclude: /node_modules/,
        use: {
          loader: "babel-loader",
          options: {
            presets: [
              [
                "@babel/preset-env",
                {
                  targets: { node: "current" },
                  useBuiltIns: "usage",
                  corejs: 3,
                  shippedProposals: true,
                },
              ],
            ],
          },
        },
      },
    ],
  },
};
