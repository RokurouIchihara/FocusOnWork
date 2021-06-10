#!/usr/bin/env python3
# -*- coding: utf8 -*-

import requests
from bs4 import BeautifulSoup


class SearchOnGoogle:

    # コンストラクタ
    def __init__(self):
        directoryPath = os.getcwd()
        self.__filename_ = 'SearchWordAndResulut.txt'
        self.__file_ = open(self.__filename_, mode='r+', encoding="utf-8")
        self.__urlRe_ = 'https://ja.wikipedia.org'
        self.__url_ = 'https://ja.wikipedia.org/w/index.php?search='
        self.__GetHtml()

    # 指定ワードをグーグルで検索し，結果を保存
    def __GetHtml(self):
        # テキストから取得
        word = self.__file_.readline().replace('\n', '')
        # 全角のスペースは消す
        word = word.replace('　', ' ')
        word = 'pubg'
        response = requests.get(self.__url_ + word)
        soup = BeautifulSoup(response.text, 'html.parser')
        if 'を新規作成しましょう。' in str(soup):
            # 直で検索結果に飛ばなかったら
            print('error')
            bodys = str(soup.find_all(
                'div', class_='mw-search-result-heading'))
            soup = BeautifulSoup(bodys, 'html.parser')
            bodys = str(soup.find_all('a'))
            # url抽出
            self.__url_ = bodys.split(
                'data-serp-pos=\"0\"')[-1].split('</a>')[0]
            self.__url_ = self.__urlRe_ + \
                self.__url_.split('href=\"')[-1].split('\"')[0]
            response = requests.get(self.__url_)
            soup = BeautifulSoup(response.text, 'html.parser')
        else:
            print('success')

        category = soup.script
        # ファイルをいったん閉じる
        self.__file_.close()
        # ファイルをリセット
        self.__file_ = open(self.__filename_, mode='w', encoding="utf-8")

        # ゲームか判定
        '''
        # 結果を書き込み
        # True: game
        # False: notgame
        '''
        if category is not None:
            self.__file_.write(str(self.__Is_game(str(category))))
        else:
            self.__file_.write(str(False))
        self.__file_.close()

    def __Is_game(self, category):
        jaGames = [
            'ゲームソフト',
            'パソコンゲーム',
            'コンピュータゲーム'
        ]

        for jaGame in jaGames:
            if jaGame in category:
                return True
        return False


searchOnGoogle = SearchOnGoogle()
