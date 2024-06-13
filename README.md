# FileHashCraft

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Third-Party Libraries

This project uses the following third-party libraries:

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) under the [MIT License](https://github.com/CommunityToolkit/dotnet/blob/main/LICENSE).
- [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/extensions) under the [MIT License](https://github.com/dotnet/extensions/blob/main/LICENSE).
- [Microsoft.Xaml.Behaviors.Wpf](https://github.com/microsoft/XamlBehaviors) under the [MIT License](https://github.com/microsoft/XamlBehaviors/blob/main/LICENSE).

## このソフトについて

FileHashCraft は ChatGPT 3.5 さんの命名です、今の所検索に引っかかってないので頂きました。
(GPT と GTP が混同してしまう人は、GTP = Go Text Protocol = 碁のテキストプロトコル、と覚えておきましょう)

一応、アプリケーションは日本語と英語に対応しています。
しかし、ドキュメントは作っていません、ドキュメントを書く所まで作れていません。

README 英語版もアプリケーション完成時にドキュメントと同時に作りたいと思っています。

## 考えている事

ざっくり

- ファイルハッシュによる重複ファイルの削除支援
- 正規表現を含めたファイルの管理支援

という機能を考えています。まだどちらも実装していません。
ファイルハッシュでの重複ファイル削除支援を優先して作る予定です。

FileHashCraft とは、このファイルをハッシュで「芸術的に」処理するとかいう過ぎた名前です。

現在プロジェクトに参画しているのは私 moriyaki と ChatGPT 3.5 様の二人です。

## 動作環境

Windows only です。それも Windows 7 での動作確認でさえ、できていない状況です。Windows 11 はまだ入れていません。
なので実質 Windows 10 only ですね。

## ライセンスについての補足

みんな大好きGPLじゃなくて、MITライセンスを適用します。
理由は簡単で「使ったソースコードの公開義務はない」のが気に入っています。

ソースコードを流用して公開(後悔とも言う)する分には

> Powered by FileHashCraft Licensed under the MIT License.

とか明記して、本プロジェクトのMITライセンス全文にアクセスできるようにしておけば、ライセンスに引っかかることもないでしょう。

ライセンス的にこの表記は必要らしいです、ブログ記事に利用するのもいいでしょう。広告記事で稼がれるのは業腹ですが。
できればプロジェクト自体へのリンクも貼っておいてくださいね。

「moriyaki is 誰」状態ですけど、moriyaki は日本語表記だと「もりゃき」です、[Mastodon](https://fedibird.com/@moriyaki) やってます。
