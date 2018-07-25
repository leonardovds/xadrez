using System;
using tabuleiro;
using xadrez_peca;

namespace Xadrez
{
    class Program
    {
        static void Main(string[] args)
        {

            try {
                Tabuleiro tab = new Tabuleiro(8, 8);

                tab.colocarPeca(new Torre(tab, Cor.Preta), new Posicao(0, 0));
                tab.colocarPeca(new Rei(tab, Cor.Preta), new Posicao(9, 0));

                Tela.imprimirTabuleiro(tab);
                
            }
            catch (TabuleiroException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.ReadLine();
        }
    }
}
