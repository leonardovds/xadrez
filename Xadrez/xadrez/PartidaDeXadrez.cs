using System.Collections.Generic;
using tabuleiro;
using System;
namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }
        public Peca vulneravelEnPassant { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            vulneravelEnPassant = null;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            colocarPecas();
        }

        public Peca executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQteMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if(pecaCapturada != null)
            {
                capturadas.Add(pecaCapturada);
            }

            //jogada especial roque

            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQteMovimentos();
                tab.colocarPeca(T, destinoT);
            }

            //jogada especial roque grande

            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(origemT);
                T.incrementarQteMovimentos();
                tab.colocarPeca(T, destinoT);
            }

            //jogada especial enPassant
            if (p is Peao)
            {
                if (origem.coluna != destino.coluna && pecaCapturada == null)
                {
                    Posicao posP;
                    if(p.cor == Cor.Branca)
                    {
                        posP = new Posicao(destino.linha + 1, destino.coluna);
                    }
                    else
                    {
                        posP = new Posicao(destino.linha - 1, destino.coluna);
                    }
                    pecaCapturada = tab.retirarPeca(posP);
                    capturadas.Add(pecaCapturada);
                }
            }

            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQteMovimentos();
            if(pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }

            tab.colocarPeca(p, origem);

            //jogada especial roque

            if (p is Rei && destino.coluna == origem.coluna + 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna + 3);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna + 1);
                Peca T = tab.retirarPeca(destinoT);
                T.decrementarQteMovimentos();
                tab.colocarPeca(T, origemT);
            }

            //jogada especial roque grande

            if (p is Rei && destino.coluna == origem.coluna - 2)
            {
                Posicao origemT = new Posicao(origem.linha, origem.coluna - 4);
                Posicao destinoT = new Posicao(origem.linha, origem.coluna - 1);
                Peca T = tab.retirarPeca(destinoT);
                T.decrementarQteMovimentos();
                tab.colocarPeca(T, origemT);
            }

            //jogada especial enPassant

            if(p is Peao)
            {
                if(origem.coluna!=destino.coluna && pecaCapturada == vulneravelEnPassant)
                {
                    Peca peao = tab.retirarPeca(destino);
                    Posicao posP;
                    if(p.cor == Cor.Branca)
                    {
                        posP = new Posicao(3, destino.coluna);
                    }
                    else
                    {
                        posP = new Posicao(4, destino.coluna);
                    }
                    tab.colocarPeca(peao, posP);
                }
            }
        }

        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executaMovimento(origem, destino);

            if (estaEmXeque(jogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Voce não pode se colocar em xeque!");
            }

            if (estaEmXeque(adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }

            if (testeXequeMate(adversaria(jogadorAtual)))
            {
                terminada = true;
            }
            else
            {
                turno++;
                mudaJogador();
            }

            Peca p = tab.peca(destino);

            //jogada especial en passant

            if (p is Peao && (destino.linha == origem.linha - 2 || destino.linha == origem.linha + 2))
            {
                vulneravelEnPassant = p;
            }
            else
            {
                vulneravelEnPassant = null;
            }
            
        }

        public void validarPosicaoOrigem(Posicao pos)
        {
            if (tab.peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça nessa posição!");
            }

            if (jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua!");
            }

            if (!tab.peca(pos).existeMovimentoPossivel())
            {
                throw new TabuleiroException("Não existem movimentos para essa peça!");
            }
        }

        public void validarPosicaoDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).podeMoverPara(destino))
            {
                throw new TabuleiroException("Não é possível mover para esse destino!");
            }
        }

        private void mudaJogador()
        {
            if (jogadorAtual == Cor.Branca)
            {
                jogadorAtual = Cor.Preta;
            }
            else
            {
                jogadorAtual = Cor.Branca;
            }
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in capturadas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }

            return aux;
        }

        public HashSet<Peca> pecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }

            aux.ExceptWith(pecasCapturadas(cor));

            return aux;
        }

        private Cor adversaria(Cor cor)
        {
            if (Cor.Branca == cor)
            {
                return Cor.Preta;
            }
            return Cor.Branca;
        }

        private Peca Rei(Cor cor)
        {
            foreach(Peca x in pecasEmJogo(cor))
            {
                if (x is Rei)
                {
                    return x;
                }
            }

            return null;
        }

        public bool testeXequeMate(Cor cor)
        {
            if (!estaEmXeque(cor))
            {
                return false;
            }

            foreach (Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for(int i = 0; i <tab.linhas; i++)
                {
                    for (int j = 0; j < tab.colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem, destino);
                            bool testeXeque = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool estaEmXeque(Cor cor)
        {
            Peca R = Rei(cor);

            if(R == null)
            {
                throw new TabuleiroException("Não há rei da cor " + cor + " no tabuleiro");
            }

            foreach (Peca x in pecasEmJogo(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if(mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }

            return false;
        }

        public void colocarNovaPeca(int linha, char coluna, Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(linha, coluna).toPosicao());
            pecas.Add(peca);
        }

        private void colocarPecas()
        {
            colocarNovaPeca(2, 'a', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(2, 'b', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(2, 'c', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(2, 'd', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(2, 'e', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(2, 'f', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(2, 'g', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(2, 'h', new Peao(tab, Cor.Branca, this));
            colocarNovaPeca(1, 'a', new Torre(tab, Cor.Branca));
            colocarNovaPeca(1, 'h', new Torre(tab, Cor.Branca));
            colocarNovaPeca(1, 'b', new Cavalo(tab, Cor.Branca));
            colocarNovaPeca(1, 'g', new Cavalo(tab, Cor.Branca));
            colocarNovaPeca(1, 'c', new Bispo(tab, Cor.Branca));
            colocarNovaPeca(1, 'f', new Bispo(tab, Cor.Branca));
            colocarNovaPeca(1, 'd', new Rainha(tab, Cor.Branca));
            colocarNovaPeca(1, 'e', new Rei(tab, Cor.Branca, this));

            colocarNovaPeca(8, 'a', new Torre(tab, Cor.Preta));
            colocarNovaPeca(8, 'h', new Torre(tab, Cor.Preta));
            colocarNovaPeca(8, 'b', new Cavalo(tab, Cor.Preta));
            colocarNovaPeca(8, 'g', new Cavalo(tab, Cor.Preta));
            colocarNovaPeca(8, 'c', new Bispo(tab, Cor.Preta));
            colocarNovaPeca(8, 'f', new Bispo(tab, Cor.Preta));
            colocarNovaPeca(8, 'd', new Rainha(tab, Cor.Preta));
            colocarNovaPeca(8, 'e', new Rei(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'a', new Peao(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'b', new Peao(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'c', new Peao(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'd', new Peao(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'e', new Peao(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'f', new Peao(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'g', new Peao(tab, Cor.Preta, this));
            colocarNovaPeca(7, 'h', new Peao(tab, Cor.Preta, this));

        }
    }
}
