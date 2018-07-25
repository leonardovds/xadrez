using System;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        private int turno;
        private Cor jogadorAtual;
        public bool terminada { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            colocarPecas();
        }

        public void executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQteMovimentos();
            Peca pecaCapturada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
        }

        private void colocarPecas()
        {
            tab.colocarPeca(new Torre(tab, Cor.Branca), new PosicaoXadrez(1, 'c').toPosicao());
            tab.colocarPeca(new Torre(tab, Cor.Branca), new PosicaoXadrez(1, 'd').toPosicao());
            tab.colocarPeca(new Torre(tab, Cor.Preta), new PosicaoXadrez(1, 'e').toPosicao());
            tab.colocarPeca(new Torre(tab, Cor.Preta), new PosicaoXadrez(1, 'f').toPosicao());
        }
    }
}
